using System;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;

namespace GHPCommerce.Modules.Inventory.Commands.Orders
{
    public class InventOrderCommandsHandler :
        ICommandHandler<UpdatePhysicalReservedQuantityCommand, ValidationResult>
    {
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ICache _redisCache;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;



        public InventOrderCommandsHandler(ICurrentOrganization currentOrganization,
            IInventoryRepository inventoryRepository,
            ICache redisCache,
            IMapper mapper,
            ICommandBus commandBus)
        {
            _currentOrganization = currentOrganization;
            _inventoryRepository = inventoryRepository;
            _redisCache = redisCache;
            _mapper = mapper;
            _unitOfWork = _inventoryRepository.UnitOfWork;
            _commandBus = commandBus;

        }

        public async Task<ValidationResult> Handle(UpdatePhysicalReservedQuantityCommand request, CancellationToken cancellationToken)
        {
            var org = default(Guid?);
            if (request.OrganizationId.IsNullOrEmpty())
                org =  await _currentOrganization.GetCurrentOrganizationIdAsync();
            else org = request.OrganizationId;
            if (org == Guid.Empty)
                throw new FluentValidation.ValidationException("Invalid user");

            var existingDim = await _inventoryRepository.Table
                .FirstOrDefaultAsync(a => a.ProductId == request.ProductId 
                                          && a.InternalBatchNumber == request.InternalBatchNumber 
                                          && a.OrganizationId == org, cancellationToken: cancellationToken);
            if (existingDim == null || org != existingDim.OrganizationId)
                throw new NotFoundException($"InventSum with product {request.ProductId} and Batch {request.InternalBatchNumber} wasn't found");
          
            existingDim.PhysicalReservedQuantity += request.Quantity;
            if (existingDim.PhysicalReservedQuantity < 0) existingDim.PhysicalReservedQuantity = 0;
            _inventoryRepository.Update(existingDim);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }
    }
}
