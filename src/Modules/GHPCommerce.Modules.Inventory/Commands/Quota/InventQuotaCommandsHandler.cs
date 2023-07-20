using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Inventory.Entities;
using GHPCommerce.Modules.Inventory.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Quota
{
    public class InventQuotaCommandsHandler : 
        ICommandHandler<ReserveDispatchedQuantityCommand, ReserveDispatchedQuantityResponse>,
        ICommandHandler<ReleaseDispatchedQuantityCommand, ReserveDispatchedQuantityResponse>
    {
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICache _redisCache;
        private readonly MedIJKModel _model;

        public InventQuotaCommandsHandler(ICurrentOrganization currentOrganization, 
            IInventoryRepository inventoryRepository, 
            ICache redisCache, MedIJKModel model)
        {
            _currentOrganization = currentOrganization;
            _inventoryRepository = inventoryRepository;
            _redisCache = redisCache;
            _model = model;
        }

        public async Task<ReserveDispatchedQuantityResponse> Handle(ReserveDispatchedQuantityCommand request, CancellationToken cancellationToken)
        {
            var validation = default(ValidationResult);
            if (request.IsDemand || _model.AXInterfacing)
                return new ReserveDispatchedQuantityResponse { ValidationResult = null, RemainedQuantity = 0 };

            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new ReserveDispatchedQuantityResponse
                {
                    ValidationResult = new ValidationResult
                    {
                        Errors =
                        {
                            new ValidationFailure("User Not valid ", "User not assigned to any organization")
                        }
                    }
                };
            await LockProvider<string>.WaitAsync(request.ProductId.ToString()  + org, cancellationToken);
            var entries = await _inventoryRepository.Table
                .Where(x => x.OrganizationId == org
                            && x.ProductId == request.ProductId
                            && x.PhysicalDispenseQuantity.HasValue
                            && x.PhysicalDispenseQuantity > 0
                            && x.ExpiryDate > DateTime.Now
                            && x.IsPublic)
                .OrderBy(x => x.ExpiryDate)
                .ToListAsync(cancellationToken);
            if (!entries.Any())
            {
                LockProvider<string>.Release(request.ProductId.ToString() + org);
                return new ReserveDispatchedQuantityResponse
                {
                    ValidationResult = new ValidationResult
                        { Errors = { new ValidationFailure("Stock ", "No stock available ") } }
                };
            }

            var initQnt = request.Quantity;
            try
            {
                var result = FifoDispatch(request, entries);
                await _inventoryRepository.UnitOfWork.SaveChangesAsync();
                string key = request.ProductId.ToString() + org;
                var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                foreach (var i in result)
                {
                    var item = inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(x =>
                        x.InternalBatchNumber == i.Key);
                    inventSum.CachedInventSumCollection.CachedInventSums[item].PhysicalOnhandQuantity += i.Value;
                }

                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
                initQnt -= result.Sum(x => x.Value);

            }
            catch (Exception e)
            {
                validation = new ValidationResult
                    { Errors = { new ValidationFailure("Validation errors ", e.Message) } };
            }
            finally
            {
                LockProvider<string>.Release(request.ProductId .ToString() + org);
            }

            return new ReserveDispatchedQuantityResponse { ValidationResult = validation, RemainedQuantity = initQnt };

        }

        private Dictionary<string, int> FifoDispatch(ReserveDispatchedQuantityCommand request, List<InventSum> entries)
        {
            var dic = new Dictionary<string, int>();
            // dispatching quantities using FIFO
            foreach (var inventSum in entries)
            {
                if (inventSum.PhysicalDispenseQuantity != null && inventSum.PhysicalDispenseQuantity.Value >= request.Quantity)
                {
                    inventSum.PhysicalOnhandQuantity += request.Quantity;
                    inventSum.PhysicalDispenseQuantity -= request.Quantity;
                    dic.Add(inventSum.InternalBatchNumber, request.Quantity);
                    _inventoryRepository.Update(inventSum);
                    break;
                }

                if (inventSum.PhysicalDispenseQuantity == null) continue;
                var dif = request.Quantity - (int)inventSum.PhysicalDispenseQuantity;
                inventSum.PhysicalOnhandQuantity += inventSum.PhysicalDispenseQuantity.Value;
                dic.Add(inventSum.InternalBatchNumber, (int)inventSum.PhysicalDispenseQuantity.Value);
                inventSum.PhysicalDispenseQuantity = 0;
                request.Quantity = dif;
                _inventoryRepository.Update(inventSum);
            }

            return dic;
        }

        public async Task<ReserveDispatchedQuantityResponse> Handle(ReleaseDispatchedQuantityCommand request, CancellationToken cancellationToken)
        {
            if ( _model.AXInterfacing)
                return new ReserveDispatchedQuantityResponse { ValidationResult = null, RemainedQuantity = 0 };

            var validation = default(ValidationResult);
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
           
            if (!org.HasValue)
                return new ReserveDispatchedQuantityResponse
                {
                    ValidationResult = new ValidationResult
                    { Errors = { new ValidationFailure("User Not valid ", "User not assigned to any organization") } }
                };
            await LockProvider<string>.WaitAsync(request.ProductId .ToString()+ org, cancellationToken);
            var entries = await _inventoryRepository.Table
                .Where(x => x.OrganizationId == org
                            && x.ProductId == request.ProductId
                            && x.ExpiryDate > DateTime.Now)
                .OrderByDescending(x => x.ExpiryDate)
                .ToListAsync(cancellationToken);
            if (!entries.Any())
            {
                LockProvider<string>.Release(request.ProductId .ToString() + org);
                return new ReserveDispatchedQuantityResponse
                {
                    ValidationResult = new ValidationResult
                    { Errors = { new ValidationFailure("Stock ", "No stock available ") } }
                };
            }

            var initQnt = request.Quantity;

            try
            {
                Dictionary<string, int> result = LifoRelease(request, entries);
                if (request.Quantity != result.Sum(x => x.Value))
                    throw new InvalidOperationException("There is something wrong. Quantity has not been fully returned ... ");

                await _inventoryRepository.UnitOfWork.SaveChangesAsync();
                string key = request.ProductId.ToString() + org;
                var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);

                foreach (var i in result)
                {
                    var index = inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(x =>
                        x.InternalBatchNumber == i.Key);
                    if (inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity >= i.Value)
                        inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity -= i.Value;
                    else inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalOnhandQuantity = 0;
                }

                await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
                initQnt -= result.Sum(x => x.Value);

            }
            catch (Exception e)
            {
                validation = new ValidationResult
                { Errors = { new ValidationFailure("Validation errors ", e.Message) } };

            }
            finally
            {
                LockProvider<string>.Release(request.ProductId .ToString() + org);
            }
            return new ReserveDispatchedQuantityResponse { ValidationResult = validation, RemainedQuantity = initQnt};
        }

        private static Dictionary<string, int> LifoRelease(ReleaseDispatchedQuantityCommand request, List<InventSum> entries)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (var inventSum in entries)
            {
                if (inventSum.PhysicalDispenseQuantity != null)
                {
                    if (request.Quantity > inventSum.PhysicalOnhandQuantity)
                    {
                        if(inventSum.PhysicalOnhandQuantity == 0)continue;
                        inventSum.PhysicalDispenseQuantity += inventSum.PhysicalOnhandQuantity;
                        request.Quantity -=(int) inventSum.PhysicalOnhandQuantity;
                        dic.Add(inventSum.InternalBatchNumber, (int)inventSum.PhysicalOnhandQuantity);
                        inventSum.PhysicalOnhandQuantity = 0;
                        continue;
                    }
                    inventSum.PhysicalDispenseQuantity += request.Quantity;
                    inventSum.PhysicalOnhandQuantity -= request.Quantity;
                    dic.Add(inventSum.InternalBatchNumber, request.Quantity);
                    break;
                }
            }

            return dic;
        }
    }
}
