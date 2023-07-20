using System;
using System.Linq;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Repositories;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Bl
{
    public class InventBlCommandsHandler :
        ICommandHandler<AddTransEntryReleaseInventCommand, ValidationResult>
    {
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IRepository<Invent, Guid> _inventRepository;
        private readonly ICache _redisCache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;



        public InventBlCommandsHandler(ICurrentOrganization currentOrganization, 
            IInventoryRepository inventoryRepository, 
            IRepository<Invent, Guid> inventRepository,
            ICache redisCache, 
            IMapper mapper, 
            ICommandBus commandBus)
        {
            _currentOrganization = currentOrganization;
            _inventoryRepository = inventoryRepository;
            _inventRepository = inventRepository;
            _redisCache = redisCache;
            _mapper = mapper;
            _unitOfWork = _inventoryRepository.UnitOfWork;
            _commandBus = commandBus;

        }

        public async Task<ValidationResult> Handle(AddTransEntryReleaseInventCommand request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                throw new FluentValidation.ValidationException("Invalid user");
            var invent = await _inventRepository.Table.AsTracking().FirstOrDefaultAsync(a => a.Id == request.InventId 
                , cancellationToken: cancellationToken);
            // var existingDim = await _inventoryRepository.Table.AsTracking().FirstOrDefaultAsync(a => a.Id == request.InventId 
            //     , cancellationToken: cancellationToken);
            var existingDim = await _inventoryRepository.Table
                .Where(x => x.ProductId == invent.ProductId
                            && x.VendorBatchNumber == invent.VendorBatchNumber
                            && x.InternalBatchNumber == invent.InternalBatchNumber
                            && x.OrganizationId == org.Value).FirstOrDefaultAsync(cancellationToken);
            if (existingDim == null || org.Value != existingDim.OrganizationId)
                throw new NotFoundException($"InventSum with product {request.InventId}  wasn't found");
            
            switch (request.Status)
            {
                // Valid BL Line : Decrease Reserved and Physical Quantity
                case  10:
                    existingDim.PhysicalOnhandQuantity -= request.Quantity;
                    existingDim.PhysicalReservedQuantity -= request.Quantity;
                    invent.PhysicalQuantity -= request.Quantity;
                    invent.PhysicalReservedQuantity -= request.Quantity;
                    break;
                // Deleted BL Line : Decrease Reserved Quantity
                case 20:
                    existingDim.PhysicalReservedQuantity -= request.Quantity;
                    break;
                // New BL Line : Decrease Physical Quantity
                case 30:
                    if(existingDim.PhysicalOnhandQuantity - request.Quantity < 0)
                        existingDim.PhysicalOnhandQuantity = 0;
                    else
                        existingDim.PhysicalOnhandQuantity -= request.Quantity;
                        invent.PhysicalQuantity -= request.Quantity;
                    break;
                // Updated BL Line : Decrease Reserved and Physical Quantity
                case 40:
                    existingDim.PhysicalOnhandQuantity -= request.Quantity;
                    //existingDim.PhysicalOnhandQuantity += request.OldQuantity - request.Quantity;
                    existingDim.PhysicalReservedQuantity -= request.OldQuantity;
                    invent.PhysicalQuantity -= request.Quantity;
                    invent.PhysicalReservedQuantity -= request.Quantity;

                    break;
                default:
                    break;
            }
            // Add transaction entry and release 
            if (existingDim.PhysicalOnhandQuantity >= request.Quantity)
            {
            }
            else
            {
                // Add Release and entry stock Transaction 
              
            }

            //Fin

            _inventoryRepository.Update(existingDim);
            _inventRepository.Update(invent);

            string key = existingDim.ProductId.ToString() + org;
            var inventSum = await _redisCache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
            var indexOfSameDim = inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(t => t.Id == existingDim.Id);
            inventSum.CachedInventSumCollection.CachedInventSums[indexOfSameDim] = _mapper.Map<CachedInventSum>(existingDim);
            await _redisCache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }
    }
}
