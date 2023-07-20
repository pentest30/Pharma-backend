using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Sales.Commands.DynamicsAx
{
    public class ReleaseReservedQuantityCommandAxHandler : ICommandHandler<ReleaseReservedQuantityCommandAx, ValidationResult>
    {
        private readonly IRepository<Order, Guid> _repository;
        private readonly ICache _cache;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly Logger _logger;

        public ReleaseReservedQuantityCommandAxHandler(IRepository<Order, Guid> repository, 
            ICache cache, 
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            Logger logger)
        {
            _repository = repository;
            _cache = cache;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(ReleaseReservedQuantityCommandAx request, CancellationToken cancellationToken)
        {
            var key = string.Empty;
            try
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (!orgId.HasValue)
                    throw new InvalidOperationException("Resources not allowed");

                var order = await _repository.Table
                    .Include(x => x.OrderItems)
                    .FirstOrDefaultAsync(x => x.OrderNumberSequence == request.OrderNumber
                                              && x.SupplierId == orgId.Value, cancellationToken: cancellationToken);
                if (order == null)
                    throw new NotFoundException($"Order with number {request.OrderNumber} was not found");
                foreach (OrderItem item in order.OrderItems.Where(x => x.ProductCode == request.ProductCode
                                                                       && x.InternalBatchNumber ==
                                                                       request.InternalBatchNumber))
                {
                    key = item.ProductId.ToString() + orgId.Value;
                    var inventSum = await _cache.GetAsync<InventSumCreatedEvent>(key, cancellationToken);
                    var index = inventSum.CachedInventSumCollection.CachedInventSums.FindIndex(x =>
                        x.ProductId == item.ProductId
                        && x.InternalBatchNumber == item.InternalBatchNumber);
                    if (index == -1)
                    {
                        continue;
                    }

                    if (inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity >=
                        item.Quantity)
                        inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity -=
                            item.Quantity;
                    else
                        inventSum.CachedInventSumCollection.CachedInventSums[index].PhysicalReservedQuantity = 0;
                    await _cache.AddOrUpdateAsync<InventSumCreatedEvent>(key, inventSum, cancellationToken);
                    item.AcceptedOnAx = request.LineReserved;
                    item.Comment = request.Comment;
                    await _commandBus.SendAsync(new ReleaseReservedQuantityCommandV2
                    {
                        InternalBatchNumber = item.InternalBatchNumber, 
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }, cancellationToken);
                }

                //_repository.Update(order);
                await _repository.UnitOfWork.SaveChangesAsync();
                return default;

            }
            catch (Exception ex)
            {
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error(ex.Message);
                _logger.Error(ex.InnerException?.Message);
                return validations;
            }
        }
    }
}