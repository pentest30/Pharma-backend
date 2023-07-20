using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Modules.Inventory.Queries;
using GHPCommerce.Modules.Inventory.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;


namespace GHPCommerce.Modules.Inventory.Commands
{

    public class InventSumCommandsHandler :
        ICommandHandler<CreateInventSumCommand, ValidationResult>,
        ICommandHandler<UpdateInventSumCommand, ValidationResult>,
        ICommandHandler<FeedInventoryCommand, ValidationResult>,
        ICommandHandler<CancelReservationsForB2BCustomerCommand, ValidationResult>,
        ICommandHandler<DeleteInventSumCommand, ValidationResult>,

        ICommandHandler<SyncCachedInventSum, ValidationResult>,
        ICommandHandler<ReserveInventoryCommand, ValidationResult>,
        ICommandHandler<CreateAXInventSumCommand, ValidationResult>,
        ICommandHandler<UpdateOnHandQuantityCommand, ValidationResult>,
        ICommandHandler<ActivateBlockInventSumCommand, ValidationResult>,
        ICommandHandler<AddQuotaQuantityCommand, ValidationResult>


    {
        private readonly IInventoryRepository _inventSumRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICache _redisCache;
        private readonly Logger _logger;
        private readonly Func<InventoryDbContext> _factory;
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly ConcurrentDictionary<string, SemaphoreSlim> _concurrentDictionary =
            new ConcurrentDictionary<string, SemaphoreSlim>();

        public InventSumCommandsHandler(IInventoryRepository inventSumRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICache redisCache,
            Logger logger,
            Func<InventoryDbContext> factory)
        {
            _inventSumRepository = inventSumRepository;
            _mapper = mapper;
            _unitOfWork = _inventSumRepository.UnitOfWork;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _redisCache = redisCache;
            _logger = logger;
            _factory = factory;
        }

        public async Task<ValidationResult> Handle(CreateInventSumCommand request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new ValidationResult
                    { Errors = { new ValidationFailure("Not valid user", "User not assigned to any organization") } };
            if (request.PhysicalOnhandQuantity < 0 || request.PhysicalReservedQuantity < 0)
                return new ValidationResult
                    { Errors = { new ValidationFailure("Quantité", "Quantité ne peut pas être négative") } };

            var inventSum = _mapper.Map<InventSum>(request);
            inventSum.OrganizationName = await _currentOrganization.GetCurrentOrganizationNameAsync();
            inventSum.OrganizationId = org.Value;
            var product = await _commandBus.SendAsync(new GetProductById { Id = request.ProductId }, cancellationToken);
            inventSum.ProductCode = product.Code;
            inventSum.ProductFullName = product.FullName;
            inventSum.PurchaseDiscountRatio /= 100;
            inventSum.SalesDiscountRatio /= 100;
            inventSum.PpaTTC = request.PpaPFS - request.PFS;
            inventSum.PpaHT = inventSum.PpaTTC / (1 + product.Tax / 100);
            inventSum.SalesDiscountRatio /= 100;
            _inventSumRepository.Add(inventSum);
            await _unitOfWork.SaveChangesAsync();
            await AddInventToCache(new InventSumCreatedEvent
            {
                Id = request.ProductId + org.ToString().ToLower(),
                CachedInventSumCollection = new CachedInventSumCollection
                    { CachedInventSums = { _mapper.Map<CachedInventSum>(inventSum) } }
            }, cancellationToken);
            return default;
        }

        private async Task AddInventToCache(InventSumCreatedEvent notification, CancellationToken cancellationToken)
        {
            var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(notification.Id, cancellationToken);
            if (inventSum == null)
            {
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(notification.Id, notification,
                    cancellationToken);

            }
            else
            {
                inventSum.CachedInventSumCollection.CachedInventSums.AddRange(notification.CachedInventSumCollection
                    .CachedInventSums);
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(notification.Id, inventSum,
                    cancellationToken);
            }
        }

        public async Task<ValidationResult> Handle(UpdateInventSumCommand request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                throw new ValidationException("Invalid user");

            var existingDim = await _inventSumRepository.Table
                .FirstOrDefaultAsync(a => a.Id == request.Id,
                cancellationToken: cancellationToken);
            if (existingDim == null || org.Value != existingDim.OrganizationId)
                throw new NotFoundException($"InventSum with {request.Id} wasn't found");
            string key = existingDim.ProductId.ToString() + org;
            var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
            var indexOfExistingDim =
                inventSum.CachedInventSumCollection.CachedInventSums
                    .FindIndex(t => t.InternalBatchNumber == existingDim.InternalBatchNumber && t.ProductId == existingDim.ProductId && existingDim.VendorBatchNumber == t.VendorBatchNumber);
            if (request.PhysicalOnhandQuantity < 0 && inventSum.CachedInventSumCollection
                .CachedInventSums[indexOfExistingDim].PhysicalReservedQuantity > 0)
                throw new ValidationException("Try later, the cached quantity has been temporarily reserved");
            existingDim.PurchaseUnitPrice = request.PurchaseUnitPrice;
            existingDim.SalesUnitPrice = request.SalesUnitPrice;
            existingDim.PurchaseDiscountRatio = request.PurchaseDiscountRatio;
            existingDim.SalesDiscountRatio = request.SalesDiscountRatio;
            existingDim.MinThresholdAlert = request.MinThresholdAlert;
            existingDim.BestBeforeDate = request.BestBeforeDate;
            existingDim.ExpiryDate = request.ExpiryDate;
            existingDim.ProductionDate = request.ProductionDate;
            existingDim.PackagingCode = request.PackagingCode;
            existingDim.packing = request.packing;
            existingDim.PhysicalDispenseQuantity = request.PhysicalDispenseQuantity;
            if (request.IsPublic.HasValue && request.IsPublic.Value != existingDim.IsPublic)
            {
                existingDim.IsPublic = request.IsPublic.Value;
                try
                {

                    var sameDim = await _inventSumRepository.Table.FirstOrDefaultAsync(
                        entryRef =>
                            entryRef.OrganizationId == existingDim.OrganizationId
                            &&
                            entryRef.ProductId == existingDim.ProductId
                            &&
                            entryRef.WarehouseId == existingDim.WarehouseId
                            &&
                            entryRef.SiteId == existingDim.SiteId
                            &&
                            entryRef.Color == existingDim.Color
                            &&
                            entryRef.Size == existingDim.Size
                            &&
                            entryRef.VendorBatchNumber == existingDim.VendorBatchNumber
                            &&
                            entryRef.InternalBatchNumber == existingDim.InternalBatchNumber
                        , cancellationToken: cancellationToken);
                    if (sameDim != null)
                    {
                        sameDim.PurchaseUnitPrice = request.PurchaseUnitPrice;
                        sameDim.SalesUnitPrice = request.SalesUnitPrice;
                        sameDim.PurchaseDiscountRatio = request.PurchaseDiscountRatio;
                        sameDim.SalesDiscountRatio = request.SalesDiscountRatio;
                        sameDim.MinThresholdAlert = request.MinThresholdAlert;
                        sameDim.BestBeforeDate = request.BestBeforeDate;
                        sameDim.ExpiryDate = request.ExpiryDate;
                        sameDim.ProductionDate = request.ProductionDate;

                        sameDim.PhysicalReservedQuantity += existingDim.PhysicalReservedQuantity;
                        sameDim.PhysicalOnhandQuantity += existingDim.PhysicalOnhandQuantity;
                        _inventSumRepository.Update(sameDim);
                        _inventSumRepository.Delete(existingDim);
                        await _unitOfWork.SaveChangesAsync();

                        var indexOfSameDim =
                            inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(t => t.Id == sameDim.Id);
                        inventSum.CachedInventSumCollection.CachedInventSums[indexOfSameDim] =
                            _mapper.Map<CachedInventSum>(sameDim);
                        inventSum.CachedInventSumCollection.CachedInventSums.RemoveAt(indexOfExistingDim);
                        await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
#if DEBUG
                        var updated2 = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
#endif
                        return default;
                    }

                }
                catch (Exception)
                {
                    // ignored
                }
            }

            _inventSumRepository.Update(existingDim);
            await _unitOfWork.SaveChangesAsync();
            inventSum.CachedInventSumCollection.CachedInventSums[indexOfExistingDim] =
                _mapper.Map<CachedInventSum>(existingDim);
            await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
#if DEBUG
            var updated = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
#endif
            return default;


        }

        public async Task<ValidationResult> Handle(DeleteInventSumCommand request, CancellationToken cancellationToken)
        {
            ValidationResult result = default;
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                throw new ValidationException("Invalid user");
            var existingDim = await _inventSumRepository.Table
                // ReSharper disable once TooManyChainedReferences
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (existingDim == null)
            {
                result = new ValidationResult();
                result.Errors.Add(new ValidationFailure("", $"InventSum with {request.Id} wasn't found"));
                return result;
            }

            string key = existingDim.ProductId.ToString() + org;
            var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);

            if (inventSum == null)
            {
                result = new ValidationResult();
                result.Errors.Add(new ValidationFailure("", $"cached invent sum was not found"));
            }

            var indexOfExistingDim = -1;
            if (inventSum != null)
            {
                indexOfExistingDim =
                    inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(t => t.Id == existingDim.Id);
                if (inventSum.CachedInventSumCollection.CachedInventSums[indexOfExistingDim].PhysicalReservedQuantity >
                    0)
                {
                    result = new ValidationResult();
                    result.Errors.Add(new ValidationFailure("", $"cached invent sum was not found"));
                }
            }

            if (existingDim.PhysicalReservedQuantity == 0)
                _inventSumRepository.Delete(existingDim);
            else
            {

                existingDim.PhysicalOnhandQuantity = existingDim.PhysicalReservedQuantity;
                _inventSumRepository.Update(existingDim);
            }

            await _unitOfWork.SaveChangesAsync();
            if (existingDim.PhysicalReservedQuantity == 0 && inventSum != null && indexOfExistingDim > -1)
                inventSum.CachedInventSumCollection.CachedInventSums.RemoveAt(indexOfExistingDim);
            else if (inventSum != null && indexOfExistingDim > -1)
            {
                inventSum.CachedInventSumCollection.CachedInventSums[indexOfExistingDim] =
                    _mapper.Map<CachedInventSum>(existingDim);
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
            }

            if (inventSum != null)
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
#if DEBUG
            await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
#endif
            return result;

        }

        public async Task<ValidationResult> Handle(FeedInventoryCommand request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                throw new ValidationException("Invalid user");
            var existingDim = await _inventSumRepository.Table
                .AsTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (existingDim == null)
                throw new NotFoundException($"InventSum with {request.Id} wasn't found");
            string key = existingDim.ProductId.ToString() + org;
            var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
            var indexOfExistingDim =
                inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(t => t.Id == existingDim.Id);
            if (request.PhysicalOnhandQuantity < 0 && inventSum.CachedInventSumCollection
                .CachedInventSums[indexOfExistingDim].PhysicalReservedQuantity > 0)
                throw new ValidationException("Try later, the cached quantity has been temporarily reserved");
            existingDim.PhysicalOnhandQuantity += request.PhysicalOnhandQuantity;
            if (existingDim.PhysicalOnhandQuantity < 0)
                throw new ValidationException("Quantité ne peut pas être négative");
            _inventSumRepository.Update(existingDim);
            await _unitOfWork.SaveChangesAsync();
            inventSum.CachedInventSumCollection.CachedInventSums[indexOfExistingDim] =
                _mapper.Map<CachedInventSum>(existingDim);
            await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
#if DEBUG
            var updated = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
#endif
            return default;
        }

        public async Task<ValidationResult> Handle(ReserveInventoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                List<InventSum> updatedEntries = new List<InventSum>();
                foreach (var reservation in request.Reservations)
                {
                    var existingDim = await _inventSumRepository.Table
                        .AsTracking()
                        .FirstOrDefaultAsync(x => x.Id == reservation.Key, cancellationToken: cancellationToken);
                    if (existingDim == null)
                        throw new NotFoundException($"InventSum with {reservation.Key} wasn't found");

                    existingDim.PhysicalReservedQuantity += reservation.Value;
                    if (existingDim.PhysicalAvailableQuantity < 0)
                        throw new ValidationException("Quantité disponible ne peut pas être négative");


                    _inventSumRepository.Update(existingDim);
                    updatedEntries.Add(existingDim);
                }

                await _unitOfWork.SaveChangesAsync();

                foreach (var entry in updatedEntries)
                {

                    string key = entry.ProductId.ToString() + entry.OrganizationId;
                    var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                    var indexOfExistingDim =
                        inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(t => t.ProductId == entry.ProductId && t.InternalBatchNumber == entry.InternalBatchNumber && t.VendorBatchNumber == entry.VendorBatchNumber);
                    inventSum.CachedInventSumCollection.CachedInventSums[indexOfExistingDim] =
                        _mapper.Map<CachedInventSum>(entry);
                    await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
                }

#if DEBUG
                var temp = updatedEntries.Select(t => t.ProductId.ToString() + t.OrganizationId).Distinct();
                foreach (var key in temp)
                {
                    var updated = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                }
#endif
                return default;
            }
            catch (Exception)
            {
                return new ValidationResult()
                    { Errors = { new ValidationFailure("ReservationError", "La réservation n'a pas été persistée") } };
            }

        }



        public async Task<ValidationResult> Handle(CancelReservationsForB2BCustomerCommand request,
            CancellationToken cancellationToken)
        {
            foreach (var reservation in request.Reservations)
            {
                var dim = _mapper.Map<InventoryDimensionExistsQuery>(reservation);
                dim.IsPublic = true;
                var res = await _commandBus.SendAsync(new GetInventSumByDimensionQuery { Dimension = dim },
                    cancellationToken);
                if (res == null)
                {
                    dim.IsPublic = false;
                    res = await _commandBus.SendAsync(new GetInventSumByDimensionQuery { Dimension = dim },
                        cancellationToken);

                }

                if (res != null)
                {
                    var inventKey = reservation.ProductId.ToString() + reservation.OrganizationId;
                    var invent = await _redisCache.GetAsync<InventSumCreatedEvent>(inventKey, cancellationToken);
                    var inv = invent.CachedInventSumCollection.CachedInventSums.FirstOrDefault(i => i.ProductId == res.ProductId && i.InternalBatchNumber == res.InternalBatchNumber);
                    inv.PhysicalReservedQuantity -= reservation.Quantity;
                    await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(inventKey, invent, cancellationToken);
                }
            }

            return new ValidationResult();
        }

        public async Task<ValidationResult> Handle(SyncCachedInventSum request, CancellationToken cancellationToken)
        {
            try
            {

                var invents = await _inventSumRepository.Table.ToListAsync(cancellationToken);
                foreach (var invent in invents)
                {
                    var inventKey = invent.ProductId.ToString() + invent.OrganizationId;
                    try
                    {
                        await LockProvider<string>.WaitAsync(inventKey, cancellationToken);
                        var productInvent = await _redisCache.GetAsync<InventSumCreatedEvent>(inventKey, cancellationToken);
                        if (productInvent == null)
                            productInvent = new InventSumCreatedEvent
                            {
                                Id = inventKey,
                                CachedInventSumCollection = new CachedInventSumCollection()
                                    { CachedInventSums = new List<CachedInventSum>() }
                            };

                        var i = productInvent.CachedInventSumCollection.CachedInventSums
                            .FindIndex(p => p.ProductId == invent.ProductId 
                            && p.InternalBatchNumber == invent.InternalBatchNumber
                            && p.OrganizationId == invent.OrganizationId);
                        if (i >= 0)
                        {
                            //Do not  override the reserved quantity on Cached Inventory reload
                            invent.PhysicalReservedQuantity = productInvent.CachedInventSumCollection.CachedInventSums[i]
                                .PhysicalReservedQuantity;
                            productInvent.CachedInventSumCollection.CachedInventSums[i] =
                                _mapper.Map<CachedInventSum>(invent);
                        }
                        else
                        {
                            productInvent.CachedInventSumCollection.CachedInventSums.Add(
                                _mapper.Map<CachedInventSum>(invent));
                        }

                        await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(inventKey, productInvent,
                            cancellationToken);
                        LockProvider<string>.Release(inventKey);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        LockProvider<string>.Release(inventKey);
                    }
                }
               
                //Clean removed inventory entries
               // await LockProvider<string>.WaitAsync("GhpCommerceDb", cancellationToken);
                var store = _redisCache.Get<Store>("GhpCommerceDb");
                if (store == null) return default;
                foreach (var invent in store.CachedKeys)
                {
                    if(!(invent is { TypeName: nameof(InventSumCreatedEvent) })) continue;
                    var inventSums = await _redisCache.GetAsync<InventSumCreatedEvent>(invent.Key, cancellationToken);
                    if (inventSums == null) continue;
                    bool _modified = false;
                    ((List<CachedInventSum>)inventSums.CachedInventSumCollection.CachedInventSums.Clone()).ForEach(x =>
                    {
                        if (invents.All(t =>
                            t.Id != x.Id))
                        {
                            var i = inventSums.CachedInventSumCollection.CachedInventSums.FindIndex(p => p.Id == x.Id);
                            if (i >= 0 && inventSums.CachedInventSumCollection.CachedInventSums.Count > i)
                            {
                                inventSums.CachedInventSumCollection.CachedInventSums.RemoveAt(i);
                                _modified = true;
                            }
                        }
                    });
                    if (_modified)
                        await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(invent.Key, inventSums,
                            cancellationToken);
                }
                

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return default;
        }

        public async Task<ValidationResult> Handle(CreateAXInventSumCommand request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (org == Guid.Empty)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("User Not valid ", "User not assigned to any organization")
                    }
                };
            }

            var product = await _commandBus.SendAsync(new GetProductByCode { CodeProduct = request.ProductCode },
                cancellationToken);
            if (product == default)
            {
                return new ValidationResult
                    { Errors = { new ValidationFailure("Produit", "Code Produit introuvable") } };

            }

            string key = product.Id.ToString() + org;
            try
            {
                await LockProvider<string>.ProvideLockObject(key).WaitAsync( cancellationToken);
                await using var dbContext = _factory.Invoke();
                var inventSum = await dbContext.Set<InventSum>()
                    .FirstOrDefaultAsync(x =>
                        x.ProductCode == request.ProductCode
                        && x.InternalBatchNumber == request.InternalBatchNumber
                        && x.OrganizationId == org.Value, cancellationToken: cancellationToken);

                if (inventSum == null)
                {
                    inventSum = _mapper.Map<InventSum>(request);
                    inventSum.OrganizationName = await _currentOrganization.GetCurrentOrganizationNameAsync();
                    inventSum.OrganizationId = org.Value;
                    inventSum.ProductCode = product.Code;
                    inventSum.ProductFullName = product.FullName;
                    inventSum.ProductId = product.Id;
                    inventSum.PurchaseDiscountRatio /= 100;
                    inventSum.PFS = request.PFS;
                    inventSum.PpaPFS = request.PpaPFS;
                    inventSum.PpaTTC = request.PpaTTC;
                    inventSum.PpaHT = request.PpaHT;
                    inventSum.SalesDiscountRatio /= 100;
                    inventSum.PackagingCode = request.PackagingCode;
                    inventSum.SalesUnitPrice = request.SalesUnitPrice;
                    inventSum.PurchaseUnitPrice = request.PurchaseUnitPrice;
                    inventSum.packing =!string.IsNullOrEmpty(request.PackagingCode)? request.PackagingCode.GetNumber() : 0;
                    await dbContext.AddAsync(inventSum, cancellationToken);
                    await dbContext.SaveChangesAsync();
                    await AddInventToCache(new InventSumCreatedEvent
                    {
                        Id = product.Id + org.ToString().ToLower(),
                        CachedInventSumCollection = new CachedInventSumCollection
                            { CachedInventSums = { _mapper.Map<CachedInventSum>(inventSum) } }
                    }, cancellationToken);
                    return default;
                }

                
                inventSum.PFS = request.PFS;
                inventSum.PpaPFS = request.PpaPFS;
                inventSum.PpaTTC = request.PpaTTC;
                inventSum.PpaHT = request.PpaHT;
                inventSum.ExpiryDate = request.ExpiryDate;
                inventSum.PurchaseUnitPrice = request.PurchaseUnitPrice;
                inventSum.PurchaseDiscountRatio = request.PurchaseDiscountRatio;
                inventSum.PurchaseDiscountRatio /= 100;
                inventSum.SalesDiscountRatio /= 100; 
                inventSum.IsPublic = request.IsPublic;
                inventSum.ProductionDate = request.ProductionDate;
                inventSum.PackagingCode = request.PackagingCode;
                inventSum.packing = !string.IsNullOrEmpty(request.PackagingCode)? request.PackagingCode.GetNumber() : 0;
                inventSum.SalesUnitPrice = request.SalesUnitPrice;
                //dbContext.Update(inventSum);
                await dbContext.SaveChangesAsync();
                var invent = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                var indexOfExistingDim = invent.CachedInventSumCollection.CachedInventSums
                    .FindIndex(t => t.ProductId == inventSum.ProductId
                                    && t.InternalBatchNumber == inventSum.InternalBatchNumber
                                    && t.OrganizationId == org);
                if (indexOfExistingDim < 0)
                {
                    await AddInventToCache(new InventSumCreatedEvent
                    {
                        Id = product.Id + org.ToString().ToLower(),
                        CachedInventSumCollection = new CachedInventSumCollection
                            { CachedInventSums = { _mapper.Map<CachedInventSum>(inventSum) } }
                    }, cancellationToken);
                }
                else
                {
                    await CheckReservationAnomalies(invent, indexOfExistingDim, inventSum, cancellationToken);
                    invent.CachedInventSumCollection.CachedInventSums[indexOfExistingDim] =_mapper.Map<CachedInventSum>(inventSum);
                    await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, invent, cancellationToken);

                }

                return default;
            }
            catch (Exception e)
            {

                return new ValidationResult
                    { Errors = { new ValidationFailure("Produit", e.Message) } };

            }
            finally
            {
                LockProvider<string>.ProvideLockObject(key).Release();
            }

        }

        private async Task CheckReservationAnomalies(InventSumCreatedEvent invent, int indexOfExistingDim, InventSum inventSum,
            CancellationToken cancellationToken)
        {
            if (invent.CachedInventSumCollection.CachedInventSums[indexOfExistingDim]
                    .PhysicalReservedQuantity - inventSum.PhysicalReservedQuantity != 0)
            {
                var anomalies =
                    await _redisCache.GetAsync<List<string>>("reservation_anomaly", cancellationToken);
                if (anomalies == null)
                {
                    await _redisCache.AddOrUpdateAsync<List<string>>("reservation_anomaly",
                        new List<string>() { inventSum.ProductCode + ";" + inventSum.InternalBatchNumber },
                        cancellationToken);
                }
                else
                {
                    anomalies.Add(inventSum.ProductCode + ";" + inventSum.InternalBatchNumber);
                    await _redisCache.AddOrUpdateAsync<List<string>>("reservation_anomaly", anomalies,
                        cancellationToken);
                }
            }
        }


        public async Task<ValidationResult> Handle(UpdateOnHandQuantityCommand request,CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (org.IsNullOrEmpty())
            {
                return new ValidationResult
                {
                    Errors = { new ValidationFailure("User Not valid ", "User not assigned to any organization") }
                };
            }

            await using var dbContext = _factory.Invoke();
            var existingDim = await dbContext
                .Set<InventSum>()
                //.AsNoTracking()
                .FirstOrDefaultAsync(x => x.InternalBatchNumber == request.InternalBatchNumber &&
                                          x.ProductCode == request.ProductCode
                                          && x.OrganizationId == org.Value,
                    cancellationToken);
            if (existingDim == null)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Stock not found",
                            $"InventSum with {request.InternalBatchNumber} wasn't found")
                    }
                };
            }
            string key = existingDim.ProductId.ToString() + org;
            try
            {
                await LockProvider<string>.ProvideLockObject(key).WaitAsync(cancellationToken);
                existingDim.PhysicalOnhandQuantity = request.PhysicalOnHandQuantity;
                await dbContext.SaveChangesAsync();
                var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                var indexOfExistingDim = inventSum.CachedInventSumCollection.CachedInventSums
                    .FindIndex(t => t.ProductId == existingDim.ProductId
                                    && t.InternalBatchNumber == existingDim.InternalBatchNumber
                                    && t.OrganizationId == org.Value);
                inventSum.CachedInventSumCollection.CachedInventSums[indexOfExistingDim].PhysicalOnhandQuantity =
                    request.PhysicalOnHandQuantity;
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
                // releases reserved quantities if the service was called by AX order reservation
                if (request.OrderNumber.HasValue && request.OrderNumber.Value > 0)
                {
                    await _commandBus.SendAsync(new ReleaseReservedQuantityCommandAx
                    {
                        ProductCode = request.ProductCode,
                        Comment = request.Comment,
                        LineReserved = request.LineReserved,
                        OrderNumber = request.OrderNumber.Value,
                        InternalBatchNumber = request.InternalBatchNumber
                    }, cancellationToken);
                }

              
                return default;

            }
            catch (Exception e)
            {
                //sem.Release();
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Stock not found",
                            e.Message)
                    }
                };
            }
            finally
            {
                LockProvider<string>.ProvideLockObject(key).Release();

            }
        }

        public async Task<ValidationResult> Handle(ActivateBlockInventSumCommand request, CancellationToken cancellationToken)
        {
            await LockProvider<string>.WaitAsync(nameof(ActivateBlockInventSumCommand), cancellationToken).ConfigureAwait(false);
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
            {
                _logger.Error("Org id was not found, command {0}", nameof(ActivateBlockInventSumCommand));
                LockProvider<string>.Release(nameof(ActivateBlockInventSumCommand));
                return new ValidationResult
                {
                    Errors = { new ValidationFailure("User Not valid ", "User not assigned to any organization") }
                };
            }

            var existingDim = await _inventSumRepository.Table
                .AsTracking()
                .FirstOrDefaultAsync(
                    x => x.OrganizationId == org
                         && x.InternalBatchNumber == request.InternalBatchNumber
                         && x.ProductCode == request.ProductCode, cancellationToken);
            if (existingDim == null)
            {
                LockProvider<string>.Release(nameof(ActivateBlockInventSumCommand));
                _logger.Error(
                    $"invent sum with {request.InternalBatchNumber}/ {request.ProductCode} was not found, command {0}",
                    nameof(ActivateBlockInventSumCommand));

                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Stock not found",
                            $"InventSum with {request.InternalBatchNumber} wasn't found")
                    }
                };
            }

            string key = existingDim.ProductId.ToString() + org;
            try
            {
                //  await LockProvider<string>.WaitAsync(key, cancellationToken);
                var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                var indexOfExistingDim = inventSum.CachedInventSumCollection.CachedInventSums
                    .FindIndex(t => t.ProductId == existingDim.ProductId
                                    && t.InternalBatchNumber == existingDim.InternalBatchNumber
                                    && t.OrganizationId == org.Value);
                existingDim.IsPublic = request.IsPublic;
                _inventSumRepository.Update(existingDim);
                await _unitOfWork.SaveChangesAsync();
                inventSum.CachedInventSumCollection.CachedInventSums[indexOfExistingDim].IsPublic = request.IsPublic;
                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
                LockProvider<string>.Release(nameof(ActivateBlockInventSumCommand));
                return default;
            }
            catch (Exception e)
            {
                LockProvider<string>.Release(nameof(ActivateBlockInventSumCommand));
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Stock not found", e.Message)
                    }
                };
            }
        }
        public async Task<ValidationResult> Handle(AddQuotaQuantityCommand request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new ValidationResult
                {
                    Errors = { new ValidationFailure("User Not valid ", "User not assigned to any organization") }
                };
            var invents = await _inventSumRepository.Table
                .Where(x => x.ProductId == request.ProductId)
                .ToListAsync(cancellationToken: cancellationToken);
            foreach (var invent in invents)
            {
                //TODO : physical reserved quantity has been removed from the formula
                invent.PhysicalDispenseQuantity = invent.PhysicalOnhandQuantity;
                invent.PhysicalOnhandQuantity = 0;
                _inventSumRepository.Update(invent);
            }

            if (invents.Any())
                await _inventSumRepository.UnitOfWork.SaveChangesAsync();
            return default;
        }
    }
}
