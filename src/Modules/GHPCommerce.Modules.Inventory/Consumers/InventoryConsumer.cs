using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Products.DTOs;
using GHPCommerce.Application.Catalog.Products.Queries;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Events.CreditNotes;
using GHPCommerce.Core.Shared.Events.DeliveryOrders;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;
using GHPCommerce.Core.Shared.Events.Orders;
using GHPCommerce.Core.Shared.Events.PreparationOrder;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Modules.Inventory.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Inventory.Consumers
{
    public class InventoryConsumer : IConsumer<InventoryMessage>,IConsumer<InventoryDecreaseMessage>, IConsumer<CreditNoteInventoryMessage>
    {
        private  InventoryDbContext _context;
        private readonly ICommandBus _commandBus;
        private readonly ICache _cache;
        private readonly Logger _logger;
        private readonly Func<InventoryDbContext> _factory;
        private readonly IMapper _mapper;
        private readonly Dictionary<Guid, int> _productsDictionary = new Dictionary<Guid, int>();
        private readonly MedIJKModel _model;

        public InventoryConsumer(ICommandBus commandBus,
            ICache cache,
            Logger logger, 
            Func<InventoryDbContext> factory,
            IMapper mapper, 
            MedIJKModel model)
        {
             // _context = context;
            _commandBus = commandBus;
            _cache = cache;
            _logger = logger;
            _factory = factory;
            _mapper = mapper;
            _model = model;

        }
        public async Task Consume(ConsumeContext<InventoryMessage> context)
        {
           
            using (_context = _factory.Invoke())
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                var trans = await _context.Database.BeginTransactionAsync();
                try
                {
                   
                    await LockProvider<Guid>.WaitAsync(context.Message.DeliveryReceiptId);
                    var quotaProducts = await _commandBus.SendAsync(new GetQuotaProductsQuery());
                    foreach (var @event in context.Message.Items)
                    {
                        await UpdateBatch(@event, context.Message.OrganizationId);
                        var result = await InventProvisioning(@event, context.Message.OrganizationId,
                            context.Message.UserId, quotaProducts);
                        // débiter la quantité entrante depuis la zone fournissuer 
                        CreateTransaction(result.Item1, result.Item2, @event.Quantity, result.Item3,
                            context.Message.RefDoc, TransactionType.Transfer, false);
                        // transaction bon de réception
                        CreateTransaction(result.Item1, result.Item2, @event.Quantity, result.Item3,
                            context.Message.RefDoc, TransactionType.SupplierReception, true);
                    }
                    await ((DbContext)_context).SaveChangesAsync();
                    await trans.CommitAsync();
                    await context.Publish<IDeliveryReceiptCompletedEvent>(new
                    {
                        context.Message.CorrelationId,
                        context.Message.DeliveryReceiptId,
                        context.Message.UserId

                    });
                    await SendProductNotification(context);
                }
                catch (Exception e)
                {
                    //
                    await trans.RollbackAsync();
                    foreach (var deliveryItem in context.Message.Items)
                    {
                        var key = deliveryItem.ProductId + context.Message.OrganizationId.ToString();
                        await LockProvider<string>.WaitAsync(key);
                        var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
                        if (inventSum != null)
                        {
                            var index = inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(x =>
                                x.ProductId == deliveryItem.ProductId &&
                                x.InternalBatchNumber == deliveryItem.InternalBatchNumber);
                            if (index > -1 && _productsDictionary.ContainsKey(deliveryItem.ProductId))
                            {
                                if (inventSum.CachedInventSumCollection.CachedInventSums[index]
                                    .PhysicalOnhandQuantity >= _productsDictionary[deliveryItem.ProductId])
                                    inventSum.CachedInventSumCollection.CachedInventSums[index]
                                        .PhysicalOnhandQuantity -= _productsDictionary[deliveryItem.ProductId];
                                else
                                    inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity = 0;
                                await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum);
                            }
                        }
                        LockProvider<string>.Release(key);
                    }
                    await context.Publish<IdeliverReceiptCancelledEvent>(new
                    {
                        context.Message.CorrelationId,
                        context.Message.OrganizationId,
                        context.Message.DeliveryReceiptId,
                        context.Message.Items,
                        context.Message.UserId
                    });
                   
                    _logger.Error(e.Message);
                    _logger.Error(e.InnerException?.Message);
                }
                finally
                { 
                    LockProvider<Guid>.Release(context.Message.DeliveryReceiptId);
                }
            }
        }
        // notifier l'utilisateur par  la nouvelle  quantité stock
        private async Task SendProductNotification(ConsumeContext<InventoryMessage> context)
        {
            foreach (var @event in context.Message.Items)
            {
                var key = @event.ProductId + context.Message.OrganizationId.ToString();
                var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
                var item = inventSum.CachedInventSumCollection.CachedInventSums
                    .Where(x =>
                    x.ProductId == @event.ProductId&&
                    (x.ExpiryDate.HasValue&&x.ExpiryDate .Value> DateTime.Now 
                    ||!x.ExpiryDate.HasValue) );
                var cachedInventSums = item as CachedInventSum[] ?? item.ToArray();
                if (cachedInventSums.Any())
                {
                    await _commandBus.SendAsync(new InventQuantityChangedCommand
                    {
                        ProductId = cachedInventSums.First().ProductId,
                        CurrentQuantity = (int)cachedInventSums.Sum(x => x.PhysicalOnhandQuantity)
                    });
                }
            }
        }


        // notifier l'utilisateur de  la nouvelle  quantité stock
        private async Task SendProductNotification(ConsumeContext<CreditNoteInventoryMessage> context)
        {
            foreach (var @event in context.Message.Items)
            {
                var key = @event.ProductId + context.Message.OrganizationId.ToString();
                var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
                var item = inventSum.CachedInventSumCollection.CachedInventSums
                    .Where(x =>
                    x.ProductId == @event.ProductId &&
                    (x.ExpiryDate.HasValue && x.ExpiryDate.Value > DateTime.Now
                    || !x.ExpiryDate.HasValue));
                var cachedInventSums = item as CachedInventSum[] ?? item.ToArray();
                if (cachedInventSums.Any())
                {
                    await _commandBus.SendAsync(new InventQuantityChangedCommand
                    {
                        ProductId = cachedInventSums.First().ProductId,
                        CurrentQuantity = (int)cachedInventSums.Sum(x => x.PhysicalOnhandQuantity)
                    });
                }
            }
        }


        private async Task <Tuple<Guid, Invent, int>> InventProvisioning( DeliveryItem item, Guid orgId, Guid userId,IEnumerable<ProductDto> quotaProducts)
        {
            var physicalQnt = 0;
            var invent = await _context.Set<Invent>().FirstOrDefaultAsync(x =>
                x.InternalBatchNumber == item.InternalBatchNumber
                && x.ProductId == item.ProductId
                && x.OrganizationId == orgId
                && x.ZoneId == Guid.Parse("7BD42E23-E657-4F99-AFEF-1AFE5CEACB16")
                && x.StockStateId ==  Guid.Parse("7BD13E23-E657-4F99-AFEF-1AFE5CEACB16"));
            if (invent == null)
                throw new NotFoundException($"invent with {item.InternalBatchNumber} was not found");
            if (invent.PhysicalQuantity >= item.Quantity)
                invent.PhysicalQuantity -= item.Quantity;
            else invent.PhysicalQuantity = 0;
            physicalQnt = 0; 
            _context.Update(invent);
            var products = quotaProducts.ToList();
            string key = invent.ProductId.ToString() + orgId;
            var inventId = default(Guid);
            // en cas de produit non quota
            if (products.All(x => x.Id != item.ProductId))
            {
                var salableInvent =  await   _context.Set<Invent>().FirstOrDefaultAsync(x =>
                    x.InternalBatchNumber == item.InternalBatchNumber
                    && x.ProductId == item.ProductId
                    && x.OrganizationId == orgId
                    && x.ZoneId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16")
                    && x.StockStateId ==  Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16"));

                // on a deux cas : si stock avec statut Zone vendable + Libéré existe déjà on met a jours la quantité vendable 
                if (salableInvent != null)
                {
                    physicalQnt =(int)salableInvent.PhysicalQuantity;
                    salableInvent.PhysicalQuantity += item.Quantity;
                    _context.Update(salableInvent);
                    inventId = salableInvent.Id;
                    var inventSumView = await _context.Set<InventSum>().FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId 
                        && x.InternalBatchNumber == salableInvent.InternalBatchNumber 
                        && x.OrganizationId == orgId);
                    if (inventSumView != null)
                    {
                        inventSumView.PhysicalOnhandQuantity += item.Quantity;
                        inventSumView.IsPublic = true;
                        _context.Set<InventSum>().Update(inventSumView);
                    }
                    else throw new NotFoundException("Invent sum was not found");
                    await LockProvider<string>.WaitAsync(key, CancellationToken.None);
                    var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
                    if (inventSum != null)
                    {
                        var index = inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(x =>
                            x.ProductId == salableInvent.ProductId &&
                            x.InternalBatchNumber == salableInvent.InternalBatchNumber);
                        if (index > -1)
                        {
                            if(_productsDictionary.ContainsKey(inventSumView.ProductId))
                                _productsDictionary[inventSumView.ProductId] = item.Quantity;
                            else
                                _productsDictionary.Add(inventSumView.ProductId, item.Quantity);
                            inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity += item.Quantity;
                            await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum);
                            
                        }
                        LockProvider<string>.Release(key);
                    }
                    else
                    {
                        LockProvider<string>.Release(key);
                        throw new NotFoundException("Invent sum was not found");
                    }
                }
                
                // sinon on ajoute une nouvelle ligne stock avec statut Zone vendable + Libéré
                else
                {
                    salableInvent =  invent.ShallowClone();
                    salableInvent.Batch = null;
                    salableInvent.Id = Guid.NewGuid();
                    salableInvent.PhysicalQuantity = item.Quantity;
                    salableInvent.ZoneId = Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16");
                    salableInvent.StockStateId = Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16");
                    salableInvent.ZoneName = "Zone vendable";
                    salableInvent.StockStateName = "Libéré";
                    salableInvent.CreatedDateTime = DateTimeOffset.Now;
                    salableInvent.CreatedByUserId = userId ;
                    salableInvent.Color = null;
                    salableInvent.Size = null;
                    _context.Entry(salableInvent).State = EntityState.Added;
                    await _context.Set<Invent>().AddAsync(salableInvent);
                    salableInvent.Batch = null;
                    inventId = salableInvent.Id;
                    var inventSum = _mapper.Map<InventSum>(salableInvent);
                    inventSum.PhysicalOnhandQuantity = item.Quantity;
                    inventSum.IsPublic = true;
                    inventSum.Color = null;
                    inventSum.Size = null;
                    inventSum.SupplierName = invent.SupplierName;
                    await _context.Set<InventSum>().AddAsync(inventSum);
                    await LockProvider<string>.WaitAsync(key, CancellationToken.None);
                    if(_productsDictionary.ContainsKey(inventSum.ProductId))
                        _productsDictionary[inventSum.ProductId] = item.Quantity;
                    else
                        _productsDictionary.Add(inventSum.ProductId, item.Quantity);
                    await CommonHelper.AddInventToCache(_cache, new InventSumCreatedEvent
                    {
                        Id = item.ProductId + orgId.ToString(),
                        CachedInventSumCollection = new CachedInventSumCollection
                            { CachedInventSums = { _mapper.Map<CachedInventSum>(inventSum) } }
                    }, CancellationToken.None);
                    LockProvider<string>.Release(key);

                }
            }
            // cas de produit quota
            else 
            {
                var nonSalableInvent =  await   _context.Set<Invent>().FirstOrDefaultAsync(x =>
                    x.InternalBatchNumber == item.InternalBatchNumber
                    && x.ProductId == item.ProductId
                    && x.OrganizationId == orgId
                    && x.ZoneId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16")
                    && x.StockStateId ==  Guid.Parse("7BD52E22-E657-4F99-AFEF-1AFE5CEACB16"));
                if (nonSalableInvent != null)
                {  
                    physicalQnt =(int)nonSalableInvent.PhysicalQuantity;
                    nonSalableInvent.PhysicalQuantity += item.Quantity;
                    _context.Update(nonSalableInvent);
                    var inventSumView = await _context.Set<InventSum>().FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId 
                        && x.InternalBatchNumber == nonSalableInvent.InternalBatchNumber 
                        && x.OrganizationId == orgId);
                    if (inventSumView != null)
                    {
                        inventSumView.PhysicalDispenseQuantity += item.Quantity;
                        _context.Set<InventSum>().Update(inventSumView);
                    }
                    else throw new NotFoundException("Invent sum was not found");
                    
                }
                else
                {
                    nonSalableInvent =  invent.ShallowClone();
                    nonSalableInvent.Batch = null;
                    nonSalableInvent.Id = Guid.NewGuid();
                    nonSalableInvent.PhysicalQuantity = item.Quantity;
                    nonSalableInvent.ZoneId = Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16");
                    nonSalableInvent.StockStateId = Guid.Parse("7BD52E22-E657-4F99-AFEF-1AFE5CEACB16");
                    nonSalableInvent.ZoneName = "Zone vendable";
                    nonSalableInvent.StockStateName = "Non libéré";
                    nonSalableInvent.CreatedDateTime = DateTimeOffset.Now;
                    nonSalableInvent.CreatedByUserId = userId ;
                    nonSalableInvent.Color = null;
                    nonSalableInvent.Size = null;
                    _context.Entry(nonSalableInvent).State = EntityState.Added;
                    await _context.Set<Invent>().AddAsync(nonSalableInvent);
                    nonSalableInvent.Batch = null;
                    var inventSum = _mapper.Map<InventSum>(nonSalableInvent);
                    inventSum.PhysicalDispenseQuantity = nonSalableInvent.PhysicalQuantity;
                    inventSum.Color = null;
                    inventSum.Size = null;
                    inventSum.IsPublic = true;
                    inventSum.SupplierName = invent.SupplierName;
                    await _context.Set<InventSum>().AddAsync(inventSum);
                }

                inventId = nonSalableInvent.Id;

            }
            return new Tuple<Guid, Invent, int>(inventId, invent, physicalQnt);
        }

        private async Task<Tuple<Guid, Invent, int>> InventProvisioning(CreditNoteItemForEvent item, Guid orgId, Guid userId, IEnumerable<ProductDto> quotaProducts)
        {



            var inventId = default(Guid);
            string key = item.ProductId.ToString() + orgId;

            var instanceInvent = await _context.Set<Invent>().FirstOrDefaultAsync(x =>
                x.InternalBatchNumber == item.InternalBatchNumber
                && x.ProductId == item.ProductId
                && x.OrganizationId == orgId
                && x.ZoneId == Guid.Parse("7BD42E22-E657-4F99-AFEF-1AFE5CEACB16")
                && x.StockStateId == Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16"));
            var physicalQnt = 0;
            if (instanceInvent != null)
            {


                physicalQnt = (int)instanceInvent.PhysicalQuantity;
                instanceInvent.PhysicalQuantity += item.Quantity;
                _context.Update(instanceInvent);
            }

            // sinon on ajoute une nouvelle ligne stock avec statut Zone non vendable + Instance
            else
            {
                instanceInvent = new Invent()
                {
                    ProductId = item.ProductId,
                    OrganizationId = orgId
                };
                var batch = await _context.Set<Batch>().FirstOrDefaultAsync(x =>
                    x.InternalBatchNumber == item.InternalBatchNumber
                    && x.ProductId == item.ProductId
                    && x.OrganizationId == orgId
                );

                if (batch == null)
                {
                    batch = new Batch()
                    {
                        OrganizationId = orgId,
                        ProductId = item.ProductId,
                        ExpiryDate = item.ExpiryDate,
                        PFS = item.PFS,
                        PpaTTC = item.Ppa,
                        InternalBatchNumber = item.InternalBatchNumber,
                        VendorBatchNumber = item.VendorBatchNumber,
                        ProductCode = item.ProductCode,
                        ProductFullName = item.ProductName,
                        CreatedDateTime = DateTimeOffset.Now,
                        CreatedByUserId = userId,
                        Id = Guid.NewGuid()

                    };
                }

                _context.Entry(batch).State = EntityState.Added;
                await _context.Set<Batch>().AddAsync(batch);
                instanceInvent.BatchId = batch == null ? Guid.Empty : batch.Id;
                instanceInvent.InternalBatchNumber =
                    batch == null ? item.InternalBatchNumber : batch.InternalBatchNumber;
                instanceInvent.VendorBatchNumber = batch == null ? item.VendorBatchNumber : batch.VendorBatchNumber;
                instanceInvent.SupplierId = batch == null ? Guid.Empty : batch.SupplierId;
                instanceInvent.SupplierName = batch == null ? "" : batch.SupplierName;
                instanceInvent.ExpiryDate = batch == null ? item.ExpiryDate : batch.ExpiryDate;

                instanceInvent.ProductFullName = batch == null ? item.ProductName : batch.ProductFullName;
                instanceInvent.ProductCode = batch == null ? item.ProductCode : batch.ProductCode;
                instanceInvent.PpaTTC = batch == null ? item.Ppa : batch.PpaTTC;
                instanceInvent.PFS = batch == null ? item.PFS : batch.PFS;

                instanceInvent.Id = Guid.NewGuid();
                instanceInvent.PhysicalQuantity = item.Quantity;
                instanceInvent.ZoneId = Guid.Parse("7BD42E22-E657-4F99-AFEF-1AFE5CEACB16");
                instanceInvent.StockStateId = Guid.Parse("7BD92E23-E657-4F99-AFEF-1AFE5CEACB16");
                instanceInvent.ZoneName = "Zone non vendable";
                instanceInvent.StockStateName = "Instance";
                instanceInvent.CreatedDateTime = DateTimeOffset.Now;
                instanceInvent.CreatedByUserId = userId;
                instanceInvent.Color = null;
                instanceInvent.Size = null;
                if (batch != null)
                {
                    instanceInvent = await _context.Set<Invent>().FirstOrDefaultAsync(x =>
  x.InternalBatchNumber == item.InternalBatchNumber
  && x.ProductId == item.ProductId
  && x.OrganizationId == orgId
  && x.ZoneId == Guid.Parse("7BD42E22-E657-4F99-AFEF-1AFE5CEACB16")
  && x.StockStateId == Guid.Parse("7BD92E23-E657-4F99-AFEF-1AFE5CEACB16"));

                    if (instanceInvent == null)
                    {
                        _context.Entry(instanceInvent).State = EntityState.Added;
                        await _context.Set<Invent>().AddAsync(instanceInvent);
                    }
                    else
                    {
                        instanceInvent.PhysicalQuantity += item.Quantity;
                        _context.Entry(instanceInvent).State = EntityState.Modified;
                        _context.Set<Invent>().Update(instanceInvent);

                    }
                }
            }

            return new Tuple<Guid, Invent, int>(instanceInvent.Id, instanceInvent, physicalQnt);
        }

        private  void CreateTransaction(Guid newInventId, Invent invent, int quantity, int currentQnt, string refDoc, TransactionType type, bool entry)
        {
            var trans = new InventItemTransaction();
            trans.InventId = newInventId;
            //trans.RefDoc = invent.InvoiceSequenceNumber;
            trans.CustomerId = invent.OrganizationId;
            trans.CustomerName = invent.OrganizationName;
            trans.OrganizationId = invent.OrganizationId;
            trans.RefDoc = refDoc;            
            //trans.OrderId = invent.in;
            trans.OrganizationName = invent.OrganizationName;
            trans.SupplierId = invent.SupplierId;
            trans.SupplierName = invent.SupplierName;
            trans.OrderDate = invent.CreatedDateTime.Date;
            trans.NewQuantity =entry? currentQnt + quantity  : currentQnt;
            // trans.OriginQuantity = entry?  invent.PhysicalQuantity + quantity : invent.PhysicalQuantity;
            trans.InternalBatchNumber = invent.InternalBatchNumber;
            trans.VendorBatchNumber = invent.VendorBatchNumber;
            trans.ProductId = invent.ProductId;
            trans.ProductFullName = invent.ProductFullName;
            trans.ProductCode = invent.ProductCode;
            trans.StockEntry = entry;
            trans.Quantity = entry? quantity : -1* quantity;
            trans.Invent = null;
            trans.CreatedDateTime = DateTimeOffset.Now;
            trans.TransactionType =  type;
            
            _context.Entry(trans).State = EntityState.Added;
             _context.Set<InventItemTransaction>().AddAsync(trans);
            trans.Invent = null;
        }
        private async Task UpdateBatch( DeliveryItem item, Guid orgId)
        {
            var batch = await _context.Set<Batch>().FirstOrDefaultAsync(x =>
                x.InternalBatchNumber == item.InternalBatchNumber &&
                x.ProductId == item.ProductId &&
                x.OrganizationId == orgId);
            if (batch == null) throw new NotFoundException($"Batch with {item.InternalBatchNumber} was not found");
            if(item.Packing>0) batch.packing = item.Packing;
            if (item.Ppa > 0) batch.PpaTTC = item.Ppa;
            _context.Update(batch);
           
        }

        public async Task Consume(ConsumeContext<InventoryDecreaseMessage> context)
        {
            using (_context = _factory.Invoke())
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                var trans = await _context.Database.BeginTransactionAsync();
                try
                {
                    await LockProvider<Guid>.WaitAsync(context.Message.DeliveryOrderId);
                    var quotaProducts = await _commandBus.SendAsync(new GetQuotaProductsQuery());
                    await UpdateInventQuantities(context.Message.OpItems,context.Message.Order, context.Message.OrganizationId, quotaProducts);
                    await GenerateAllDeliveryTransactions(context.Message.DeliveryOrder.DeleiveryOrderItems,
    context.Message.DeliveryOrder, context.Message.OrganizationId, quotaProducts, context.Message.UserId);

                    await ((DbContext)_context).SaveChangesAsync();
                    await trans.CommitAsync();
                    await context.Publish<IDeliveryOrderCompletedEvent>(new
                    {
                        context.Message.CorrelationId,
                        context.Message.DeliveryOrderId,
                        context.Message.UserId
                    });
                }
                catch (Exception e)
                {
                    //
                    await trans.RollbackAsync();
                    foreach (var deliveryItem in context.Message.DeliveryOrder.DeleiveryOrderItems)
                    {
                        var key = deliveryItem.ProductId + context.Message.OrganizationId.ToString();
                        await LockProvider<string>.WaitAsync(key);
                        var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
                        if (inventSum != null)
                        {
                            var index = inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(x =>
                                x.ProductId == deliveryItem.ProductId &&
                                x.InternalBatchNumber == deliveryItem.InternalBatchNumber);
                            if (index > -1)
                            {
                                inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity += deliveryItem.Quantity;
                                await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum);
                            }
                        }
                        LockProvider<string>.Release(key);

                    }
                    await context.Publish<IDeliveryOrderCancelledEvent>(new
                    {
                        context.Message.CorrelationId,
                        context.Message.OrganizationId,
                        context.Message.DeliveryOrderId,
                        context.Message.DeliveryOrder.DeleiveryOrderItems,
                        context.Message.UserId

                    });
                   
                    _logger.Error(e.Message);
                    _logger.Error(e.InnerException?.Message);
                }
                finally{ 
                    LockProvider<Guid>.Release(context.Message.DeliveryOrderId);
                }
            }
        }

        private async Task UpdateInventQuantities(List<PreparationOrderItem> messageOpItems, Order messageOrder, Guid OrganizationId, IEnumerable<ProductDto> quotaProducts)
        {
            foreach (var item in messageOpItems)
            {
                // en cas de produit non quota
                // var productNotQuota = quotaProducts.ToList().All(x => x.Id != item.ProductId);

                var invent =  await   _context.Set<Invent>().FirstOrDefaultAsync(x =>
            
                    x.ProductId == item.ProductId &&
                    x.ZoneId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16")  &&
                    x.StockStateId == Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16") &&
                    x.InternalBatchNumber == item.InternalBatchNumber &&
                    x.VendorBatchNumber == item.VendorBatchNumber &&
                    x.OrganizationId == OrganizationId
                );
                
              
                var orderItem = messageOrder.OrderItems.Find(orderItem =>
                    orderItem.ProductId == item.ProductId && 
                    orderItem.InternalBatchNumber == item.InternalBatchNumber && 
                    orderItem.VendorBatchNumber == item.VendorBatchNumber);
                var inventOfNewLine = new Invent();
                if (orderItem == null)
                {
                    inventOfNewLine =  await   _context.Set<Invent>().FirstOrDefaultAsync(x =>
                
                        x.ProductId == item.ProductId &&
                        x.ZoneId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16") &&
                        x.StockStateId == Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16") &&
                        x.InternalBatchNumber == item.InternalBatchNumber &&
                        x.VendorBatchNumber == item.VendorBatchNumber &&
                        x.OrganizationId == OrganizationId
                    );
                 
                }
                var command = new AddTransEntryReleaseInventCommand() { };
                command.Status = (int)item.Status;
                command.InventId = (invent != null) ?  invent.Id : new Guid();
                command.Quantity = item.Quantity;
                command.OldQuantity = (orderItem == null) ? item.Quantity :orderItem.Quantity;
                var inventSumView = await _context.Set<InventSum>().FirstOrDefaultAsync(x =>
                    x.ProductId == item.ProductId
                    && x.VendorBatchNumber ==
                    ((orderItem == null) ? item.VendorBatchNumber : orderItem.VendorBatchNumber)
                    && x.InternalBatchNumber == ((orderItem == null) ? item.InternalBatchNumber :orderItem.InternalBatchNumber)
                    && x.OrganizationId == OrganizationId);
                if (inventSumView == null)
                    throw new NotFoundException($"InventSum with product {invent.Id}  wasn't found");

                switch (item.Status)
                {
                    // Valid BL Line : Decrease Reserved and Physical Quantity
                    case 10:
                        inventSumView.PhysicalOnhandQuantity -= item.Quantity;
                        inventSumView.PhysicalReservedQuantity -= item.Quantity;
                        //if (!productNotQuota) inventSumView.PhysicalDispenseQuantity -= item.Quantity;   
                        if(invent != null) invent.PhysicalQuantity -= item.Quantity;
                        break;
                    // Deleted BL Line : Decrease Reserved Quantity
                    case 20:
                        inventSumView.PhysicalReservedQuantity -= item.Quantity;
                        break;
                    // New BL Line : Decrease Physical Quantity
                    case 30:
                        if (inventSumView.PhysicalOnhandQuantity - item.Quantity < 0)
                            inventSumView.PhysicalOnhandQuantity = 0;

                        else
                        {
                            inventSumView.PhysicalOnhandQuantity -= item.Quantity;
                            //if (!productNotQuota) inventSumView.PhysicalDispenseQuantity -= item.Quantity;
                        }
                        if (inventOfNewLine != null)   inventOfNewLine.PhysicalQuantity -= item.Quantity; 

                        break;
                    case 40:
                        inventSumView.PhysicalOnhandQuantity -= item.Quantity;
                        inventSumView.PhysicalReservedQuantity -= item.OldQuantity ;
                        //if (!productNotQuota) inventSumView.PhysicalDispenseQuantity -= item.Quantity;   

                        if(invent != null) invent.PhysicalQuantity -= item.Quantity;

                        break;
                    default:
                        break;
                }

                //Fin

                _context.Update(inventSumView);
                if(invent != null)
                    _context.Update(invent);
                if(inventOfNewLine != null && inventOfNewLine.Id != Guid.Empty ) _context.Update(inventOfNewLine);

                string key = inventSumView.ProductId.ToString() + OrganizationId;
                await LockProvider<string>.WaitAsync(key);
                var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
                var indexOfSameDim =
                    inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(t => t.ProductId == inventSumView.ProductId && t.InternalBatchNumber == inventSumView.InternalBatchNumber && t.VendorBatchNumber == inventSumView.VendorBatchNumber);
                inventSum.CachedInventSumCollection.CachedInventSums[indexOfSameDim] =
                    _mapper.Map<CachedInventSum>(inventSumView);
                await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, CancellationToken.None);
                LockProvider<string>.Release(key);
            }

        }

        private async Task GenerateAllDeliveryTransactions(List<DeliveryOrderItem> deliveryOrderItems,DeliveryOrder deliveryOrder,Guid organizationId , IEnumerable<ProductDto> quotaProducts, Guid? userId = null)
        {
            foreach (var @event in deliveryOrderItems)
            {
                // en cas de produit non quota
                var productNotQuota = quotaProducts.ToList().All(x => x.Id != @event.ProductId);

                var invent =  await   _context.Set<Invent>().FirstOrDefaultAsync(x =>
                
                    x.ProductId == @event.ProductId &&
                    x.ZoneId == (productNotQuota ? Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16") : Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16")) &&
                    x.StockStateId == (productNotQuota ? Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16") : Guid.Parse("7BD52E22-E657-4F99-AFEF-1AFE5CEACB16")) &&
                    x.InternalBatchNumber == @event.InternalBatchNumber &&
                    x.VendorBatchNumber == @event.VendorBatchNumber &&
                    x.OrganizationId == organizationId
                );
                if (invent != null) await GenerateDeliveryOrderTransaction(@event, deliveryOrder, invent, userId);
            }
        }
        private async Task GenerateDeliveryOrderTransaction(DeliveryOrderItem item, DeliveryOrder deliveryOrder, Invent invent, Guid? userId = null)
        {
            var trans = new InventItemTransaction { };
            trans.InventId = invent.Id;
            trans.RefDoc = deliveryOrder.DeleiveryOrderNumber;
            trans.ProductCode = item.ProductCode;
            trans.ProductFullName = item.ProductName;
            trans.ProductId = item.ProductId;
            trans.CustomerId = deliveryOrder.CustomerId;
            trans.CustomerName = deliveryOrder.CustomerName;
            trans.OrderId = deliveryOrder.OrderId;
            trans.SupplierId = deliveryOrder.SupplierId;
            trans.OrderDate = deliveryOrder.OrderDate;
            trans.InternalBatchNumber = item.InternalBatchNumber;
            trans.VendorBatchNumber = item.VendorBatchNumber;
            trans.NewQuantity = invent.PhysicalQuantity - item.Quantity;
            trans.OriginQuantity = invent.PhysicalQuantity;
            trans.Quantity = -1 * item.Quantity;
            trans.StockEntry = false;
            trans.CreatedDateTime = DateTime.Now;
            trans.CreatedByUserId = (Guid)(userId ?? null);
            trans.TransactionType = TransactionType.DeliveryNote;
            trans.OrganizationId = deliveryOrder.OrganizationId; 
            trans.OrganizationName = invent.OrganizationName;
            _context.Entry(trans).State = EntityState.Added;
            await _context.Set<InventItemTransaction>().AddAsync(trans);
        }

        public async Task Consume(ConsumeContext<CreditNoteInventoryMessage> context)
        {
            using (_context = _factory.Invoke())
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = false;
                var trans = await _context.Database.BeginTransactionAsync();
                try
                {

                    await LockProvider<Guid>.WaitAsync(context.Message.CreditNoteId);
                    var quotaProducts = await _commandBus.SendAsync(new GetQuotaProductsQuery());
                    foreach (var @event in context.Message.Items)
                    {
                        
                        var result = await InventProvisioning(@event, context.Message.OrganizationId,
                            context.Message.UserId, quotaProducts);
                        //// débiter la quantité entrante depuis la zone fournissuer 
                        //CreateTransaction(result.Item1, result.Item2, @event.Quantity, result.Item3,
                        //    context.Message.RefDoc, TransactionType., false);
                        // transaction bon de retour
                        CreateTransaction(result.Item1, result.Item2, @event.Quantity, result.Item3,
                            context.Message.RefDoc, TransactionType.CustomerReturn, true);
                    }
                    await ((DbContext)_context).SaveChangesAsync();
                    await trans.CommitAsync();
                   
                    await SendProductNotification(context);
                }
                catch (Exception e)
                {
                    //
                    await trans.RollbackAsync();
 
                }
                finally
                {
                    LockProvider<Guid>.Release(context.Message.CreditNoteId);
                }
            }
        }
    }
}