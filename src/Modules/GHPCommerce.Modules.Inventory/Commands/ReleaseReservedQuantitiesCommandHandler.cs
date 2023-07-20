using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Inventory.Commands
{
    public class ReleaseReservedQuantitiesCommandHandler :
        ICommandHandler<ReleaseReservedQuantityCommand, ValidationResult>,
        ICommandHandler<ReleaseReservedQuantityCommandV2, ValidationResult>,
        ICommandHandler<ReleaseReservedQuantityCommandV3, ValidationResult>
    {
        private readonly IRepository<InventSum, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly Logger _logger;

        public ReleaseReservedQuantitiesCommandHandler(IRepository<InventSum, Guid> repository,
            ICurrentOrganization currentOrganization, Logger logger)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(ReleaseReservedQuantityCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!orgId.HasValue)
                return new ValidationResult
                {
                    Errors = { new ValidationFailure("User Not valid ", "User not assigned to any organization") }
                };
            var ids = request.ReleasedQuantities.Select(x => x.Key);
            var items = await _repository.Table
                .Where(x => ids.Any(p => p == x.Id) 
                            && x.OrganizationId == orgId.Value)
                .ToListAsync(cancellationToken: cancellationToken);
            foreach (var item in items)
            {
                if (item.PhysicalReservedQuantity >= request.ReleasedQuantities[item.Id])
                    item.PhysicalReservedQuantity -= request.ReleasedQuantities[item.Id];
                
                else item.PhysicalReservedQuantity = 0;
               // _repository.Update(item);
            }

            await _repository.UnitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(ReleaseReservedQuantityCommandV2 request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!orgId.HasValue)
                return new ValidationResult
                {
                    Errors = { new ValidationFailure("User Not valid ", "User not assigned to any organization") }
                };
            var item = await _repository.Table
                .Where(x => x.ProductId == request.ProductId
                            &&x.InternalBatchNumber == request.InternalBatchNumber
                            && x.OrganizationId == orgId.Value)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if(item!=null)
            {
                if (item.PhysicalReservedQuantity >= request.Quantity)
                    item.PhysicalReservedQuantity -= request.Quantity;
                else item.PhysicalReservedQuantity = 0;
                await _repository.UnitOfWork.SaveChangesAsync();
            }
            return default;
        }
        public async Task<ValidationResult> Handle(ReleaseReservedQuantityCommandV3 request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            
            foreach (var itemRequest in request.QuantitiesToRelease)
            {
                var item = await _repository.Table
                    .Where(x => x.ProductId == itemRequest.ProductId
                                &&x.InternalBatchNumber == itemRequest.InternalBatchNumber
                                && x.OrganizationId == request.OrganizationId)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
                if(item!=null)
                {
                    try
                    {
                        await LockProvider<string>.ProvideLockObject(item.ProductId + orgId.Value.ToString())
                            .WaitAsync(cancellationToken);
                        if (item.PhysicalReservedQuantity >= itemRequest.Quantity)
                            item.PhysicalReservedQuantity -= itemRequest.Quantity;
                        else item.PhysicalReservedQuantity = 0;
                        await _repository.UnitOfWork.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        _logger.Error($"error on handler  {nameof(ReleaseReservedQuantitiesCommandHandler)},position 104,   message :"+  e.Message );
                        
                        //throw;
                    }
                    finally
                    {
                        LockProvider<string>.ProvideLockObject(item.ProductId + orgId.Value.ToString()).Release();
                    }
                }
                
            }
            
            return default;
        }
    }
}