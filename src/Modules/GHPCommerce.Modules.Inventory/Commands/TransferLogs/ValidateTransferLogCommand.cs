using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Modules.Inventory.Repositories;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Inventory.Commands.TransferLogs
{
    public class ValidateTransferLogCommand : SaveTransferLogCommand
    {
    }
     public  class ValidateTransferLogCommandHandler : ICommandHandler<ValidateTransferLogCommand, ValidationResult>
     {
         private readonly ICurrentOrganization _currentOrganization;
         private readonly IMapper _mapper;
         private readonly Logger _logger;
         private readonly Func<InventoryDbContext> _factory;
         private readonly ICache _redisCache;
         private readonly ICommandBus _commandBus;
         private  InventoryDbContext _context;
         private readonly Dictionary<Guid, int> _changedQuantities = new Dictionary<Guid, int>();

         public ValidateTransferLogCommandHandler(
             ICurrentOrganization currentOrganization,
             IMapper mapper,
             Logger logger, 
             Func<InventoryDbContext> factory, 
             ICache redisCache, 
             ICommandBus commandBus )
         {
             _currentOrganization = currentOrganization;
             _mapper = mapper;
             _logger = logger;
             _factory = factory;
             _redisCache = redisCache;
             _commandBus = commandBus;
         }
         public async Task<ValidationResult> Handle(ValidateTransferLogCommand request, CancellationToken cancellationToken)
         {
             var validations =  new ValidationResult();
             using (_context = _factory.Invoke())
             {
                 _context.ChangeTracker.AutoDetectChangesEnabled = false;
                 var trans = await _context.Database.BeginTransactionAsync(cancellationToken);

                 try
                 {
                     var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                     if (orgId == default) throw new InvalidOperationException("Resources not allowed");
                     var transferLog = await _context.Set<TransferLog>()
                         .Include(x=>x.Items)
                         //.Include("Items.Invent")
                         .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
                     if (transferLog == null)
                         throw new NotFoundException($"Transfer log with  {request.Id} was not found");
                     if(!transferLog.Items.Any())
                         throw new NotFoundException($"Journal de transfer sans lignes");

                         //vérifier d'abbord si les quantités sont disponible
                     await CheckAvailabilityOfQuantities( transferLog, orgId, cancellationToken);
                     foreach (var transferLogItem in transferLog.Items)
                     {
                         var invent = await _context.Set<Invent>()
                             .FirstOrDefaultAsync(x => x.ZoneId == transferLog.ZoneSourceId
                                                       && x.StockStateId == transferLog.StockStateSourceId
                                                       && x.InternalBatchNumber == transferLogItem.InternalBatchNumber 
                                                       && x.ProductId == transferLogItem.ProductId
                                                       && x.OrganizationId == orgId, cancellationToken: cancellationToken);
                         string key = transferLogItem.ProductId.ToString() + orgId;
                         var sum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
                         var item = sum.CachedInventSumCollection.CachedInventSums.First(x =>
                             x.InternalBatchNumber == transferLogItem.InternalBatchNumber &&
                             x.ProductId == transferLogItem.ProductId);
                         try
                         {
                             
                             await LockProvider<string>.WaitAsync(transferLogItem.ProductId + orgId.Value.ToString(), cancellationToken);
                             invent.PhysicalQuantity -= transferLogItem.Quantity;
                             _context.Update(invent);
                             // transaction pour débiter la quantité
                             CreateTransaction(transferLogItem.InventId, invent,(int) transferLogItem.Quantity*-1,
                                 (int)invent.PhysicalQuantity, transferLog.DocumentRef,TransactionType.ManualTransfer, false);
                             var newInvent = await NewInvent(transferLog, transferLogItem, orgId, invent, cancellationToken);

                             CreateTransaction(newInvent.Id, invent,(int) transferLogItem.Quantity,
                                 (int)invent.PhysicalQuantity, transferLog.DocumentRef,TransactionType.ManualTransfer, false);
                             // en cas de zone source égale  zone vendable + libéré on met  a jour invent sum : quantité (-)
                             if (transferLog.ZoneSourceId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16") && transferLog.StockStateSourceId== Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16")) 
                                 await UpdateOnHandQuantityOfInventSum(invent,transferLogItem.InternalBatchNumber, transferLogItem.ProductId,(int) transferLogItem.Quantity* -1, orgId, cancellationToken);
                             // en cas de zone de source égale  zone vendable +  non libéré on met  a jour invent sum: quantité non vendable (-)
                             if (transferLog.ZoneSourceId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16") && transferLog.StockStateSourceId == Guid.Parse("7BD52E22-E657-4F99-AFEF-1AFE5CEACB16")) 
                                 await UpdateNotSalableQntInventSum(invent,transferLogItem.InternalBatchNumber, transferLogItem.ProductId,(int) transferLogItem.Quantity*-1, orgId, cancellationToken);
                             // en cas de zone dest égale  zone vendable + libéré on met  a jour invent sum : quantité (+)
                             if (transferLog.ZoneDestId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16") && transferLog.StockStateId == Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16")) 
                                 await UpdateOnHandQuantityOfInventSum(invent,transferLogItem.InternalBatchNumber, transferLogItem.ProductId,(int) transferLogItem.Quantity, orgId, cancellationToken);
                             // en cas de zone dest égale  zone vendable + non libéré on met  a jour invent sum : quantité non vendable (+)
                             if (transferLog.ZoneDestId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16") && transferLog.StockStateId == Guid.Parse("7BD52E22-E657-4F99-AFEF-1AFE5CEACB16")) 
                                 await UpdateNotSalableQntInventSum(invent,transferLogItem.InternalBatchNumber, transferLogItem.ProductId,(int) transferLogItem.Quantity, orgId, cancellationToken);
                             int totalQnt =(int) sum.CachedInventSumCollection.CachedInventSums
                                 .Where(x => x.ProductId == item.ProductId).Sum(x => x.PhysicalAvailableQuantity);

                             if (!_changedQuantities.ContainsKey(item.ProductId))
                                 _changedQuantities.Add(item.ProductId, totalQnt);
                             else _changedQuantities[item.ProductId] = totalQnt;
                             LockProvider<string>.Release(transferLogItem.ProductId + orgId.Value.ToString());
                         }
                         catch (Exception e)
                         {
                             LockProvider<string>.Release(transferLogItem.ProductId + orgId.Value.ToString());
                             Console.WriteLine(e);
                             throw e;
                         }
                     }

                     transferLog.Status = TransferLogStatus.Valid;
                     _context.Update(transferLog);
                     await _context.SaveChangesAsync();
                     await trans.CommitAsync(cancellationToken);
                     foreach (var keyValuePair in _changedQuantities)
                     {
                         await _commandBus.SendAsync(new InventQuantityChangedCommand { ProductId = keyValuePair.Key, CurrentQuantity = keyValuePair.Value }, cancellationToken);
                     }
                 }
                 catch (Exception ex)
                 {
                     validations = new ValidationResult { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                     _logger.Error(ex.Message);
                     _logger.Error(ex.InnerException?.Message);
                      await trans.RollbackAsync(cancellationToken);
                     
                 }
             }
             
             return validations;
         }

         private async Task CheckAvailabilityOfQuantities( TransferLog transferLog, Guid? orgId,CancellationToken cancellationToken)
         {
             foreach (TransferLogItem transferLogItem in transferLog.Items)
             {
                 string key = transferLogItem.ProductId.ToString() + orgId;
                 await LockProvider<string>.WaitAsync(key, cancellationToken);

                 var sum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
                 var item = sum.CachedInventSumCollection.CachedInventSums.FirstOrDefault(x =>
                     x.InternalBatchNumber == transferLogItem.InternalBatchNumber &&
                     x.ProductId == transferLogItem.ProductId);
                 var invent = await _context.Set<Invent>()
                     .FirstOrDefaultAsync(x => x.ZoneId == transferLog.ZoneSourceId
                                               && x.StockStateId == transferLog.StockStateSourceId
                                               && x.InternalBatchNumber == transferLogItem.InternalBatchNumber
                                               && x.ProductId == transferLogItem.ProductId
                                               && x.OrganizationId == orgId, cancellationToken: cancellationToken);

                 if (invent == null || item == null
                                    || transferLog.ZoneSourceId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16")
                                    && transferLog.StockStateSourceId == Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16")
                                    && (invent.PhysicalQuantity < transferLogItem.Quantity
                                        || item.PhysicalAvailableQuantity < transferLogItem.Quantity))
                 {
                     LockProvider<string>.Release(key);
                     throw new InvalidOperationException(
                         $"Quantité non disponible sur le stock {transferLogItem.InternalBatchNumber}, Quantité disponible actuellement {item?.PhysicalAvailableQuantity}");
                     //continue;
                 }
                 LockProvider<string>.Release(key);
             }
         }

         private async Task UpdateNotSalableQntInventSum(Invent invent,string internalBatchNumber, Guid productId,int qnt, Guid? orgId,CancellationToken cancellationToken)
         {
             var inventSum = await _context.Set<InventSum>()
                 .FirstOrDefaultAsync(x => x.InternalBatchNumber == internalBatchNumber
                                           && x.ProductId == productId
                                           && x.OrganizationId == orgId, cancellationToken: cancellationToken);
             if (inventSum != null)
             {
                 inventSum.PhysicalDispenseQuantity ??= 0;
                 if (inventSum.PhysicalDispenseQuantity + qnt >= 0)
                     inventSum.PhysicalDispenseQuantity += qnt;
                 else inventSum.PhysicalDispenseQuantity = 0;
                 _context.Update(inventSum);
             }
             else
             {
                 inventSum = _mapper.Map<InventSum>(invent);
                 inventSum.Id = Guid.NewGuid();
                 inventSum.PhysicalDispenseQuantity = qnt;
                 inventSum.IsPublic = true;
                 inventSum.Color = null;
                 inventSum.Size = null;
                 _context.Set<InventSum>().Add(inventSum);
             }
         }

         private async Task UpdateOnHandQuantityOfInventSum(Invent invent,string internalBatchNumber, Guid productId,int qnt, Guid? orgId,CancellationToken cancellationToken)
         {
             var inventSum = await _context.Set<InventSum>()
                 .FirstOrDefaultAsync(x => x.InternalBatchNumber == internalBatchNumber
                                           && x.ProductId == productId
                                           && x.OrganizationId == orgId, cancellationToken: cancellationToken);
             if (inventSum != null)
             {
                 inventSum.PhysicalDispenseQuantity ??= 0;
                 if (inventSum.PhysicalOnhandQuantity + qnt >= 0)
                     inventSum.PhysicalOnhandQuantity += qnt;
                 else inventSum.PhysicalOnhandQuantity = 0;
                 _context.Update(inventSum);
             }

             else
             {
                 inventSum = _mapper.Map<InventSum>(invent);
                 inventSum.Id = Guid.NewGuid();
                 inventSum.PhysicalOnhandQuantity = qnt;
                 inventSum.IsPublic = true;
                 inventSum.Color = null;
                 inventSum.Size = null;
                 _context.Set<InventSum>().Add(inventSum);
             }
             string key = productId.ToString() + orgId;
             //await LockProvider<string>.WaitAsync(key, cancellationToken);
             var sum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, CancellationToken.None);
             if (sum != null)
             {
                 var index = sum.CachedInventSumCollection.CachedInventSums.FindIndex(x =>
                     x.ProductId == inventSum.ProductId &&
                     x.InternalBatchNumber == inventSum.InternalBatchNumber);
                 if (index > -1)
                 {
                     if(sum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity + qnt>=0)
                        sum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity +=qnt;
                     else
                         sum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity = 0;
                     await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, sum, cancellationToken);
                 }

                 LockProvider<string>.Release(key);
             }
             else
             {
                 await CommonHelper.AddInventToCache(_redisCache, new InventSumCreatedEvent
                 {
                     Id = productId + orgId.ToString(),
                     CachedInventSumCollection = new CachedInventSumCollection
                         { CachedInventSums = { _mapper.Map<CachedInventSum>(inventSum) } }
                 }, CancellationToken.None);
                 LockProvider<string>.Release(key);
              
             }
         }

         private async Task<Invent> NewInvent(TransferLog transferLog, TransferLogItem transferLogItem, Guid? orgId, Invent invent,CancellationToken cancellationToken)
         {
             var newInvent = await _context.Set<Invent>()
                 .FirstOrDefaultAsync(x => x.ZoneId == transferLog.ZoneDestId
                                           && x.StockStateId == transferLog.StockStateId
                                           && x.InternalBatchNumber == transferLogItem.InternalBatchNumber
                                           && x.ProductId == transferLogItem.ProductId
                                           && x.OrganizationId == orgId, cancellationToken: cancellationToken);
             if (newInvent == null)
             {
                 newInvent = invent.ShallowClone();
                 newInvent.Id = Guid.NewGuid();
                 newInvent.ZoneId = transferLog.ZoneDestId;
                 newInvent.ZoneName = transferLog.ZoneDestName;
                 newInvent.PhysicalQuantity = transferLogItem.Quantity;
                 newInvent.StockStateId = transferLog.StockStateId;
                 newInvent.StockStateName = transferLog.StockStateName;
                 _context.Set<Invent>().Add(newInvent);
             }
             else
             {
                 newInvent.PhysicalQuantity += transferLogItem.Quantity;
                 newInvent.ZoneId = transferLog.ZoneDestId;
                 newInvent.ZoneName = transferLog.ZoneDestName;
                 newInvent.StockStateId = transferLog.StockStateId;
                 newInvent.StockStateName = transferLog.StockStateName;
                 _context.Set<Invent>().Update(newInvent);
             }

             return newInvent;
         }

         private void CreateTransaction(Guid newInventId, Invent invent, int quantity, int currentQnt, string refDoc, TransactionType type, bool entry)
         {
             var trans = new InventItemTransaction();
             trans.InventId = newInventId;
             trans.CustomerId = invent.OrganizationId;
             trans.CustomerName = invent.OrganizationName;
             trans.OrganizationId = invent.OrganizationId;
             trans.RefDoc = refDoc;
             trans.OrganizationName = invent.OrganizationName;
             trans.SupplierId = invent.SupplierId;
             trans.SupplierName = invent.SupplierName;
             trans.OrderDate = invent.CreatedDateTime.Date;
             trans.NewQuantity =entry? currentQnt + quantity  : currentQnt;
             trans.InternalBatchNumber = invent.InternalBatchNumber;
             trans.VendorBatchNumber = invent.VendorBatchNumber;
             trans.ProductId = invent.ProductId;
             trans.ProductFullName = invent.ProductFullName;
             trans.ProductCode = invent.ProductCode;
             trans.StockEntry = entry;
             trans.Quantity =  quantity;
             trans.Invent = null;
             trans.CreatedDateTime = DateTimeOffset.Now;
             trans.TransactionType =  type;
             _context.Entry(trans).State = EntityState.Added;
             _context.Set<InventItemTransaction>().Add(trans);
             trans.Invent = null;
         }
     }
}