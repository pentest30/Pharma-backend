using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Orders;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Sales.Commands.Orders;
using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Sales.Commands.DynamicsAx
{
    public class OrderAxCreatedCommandHandler :ICommandHandler<OrderAxCreatedCommand, ValidationResult>
    {
        private readonly IRepository<Order, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICache _cache;
        private readonly Logger _logger;
        private readonly ICommandBus _commandBus;
        public OrderAxCreatedCommandHandler(IRepository<Order, Guid> repository, 
            ICurrentOrganization currentOrganization, 
            ICache cache,
            Logger logger, 
            ICommandBus commandBus)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _cache = cache;
            _logger = logger;
            _commandBus = commandBus;
        }

        public async Task<ValidationResult> Handle(OrderAxCreatedCommand request, CancellationToken cancellationToken)
        {
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
                //  order.OrderStatus =  OrderStatus.CreatedOnAx;
                order.CodeAx = request.CodeAx;
                _repository.Update(order);
                await _repository.UnitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error(ex.Message);
                _logger.Error(ex.InnerException?.Message);
                return validations;
            }

            return default;
        }
    }
}