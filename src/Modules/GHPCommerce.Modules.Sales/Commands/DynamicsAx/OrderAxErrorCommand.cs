using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.Commands;
using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Commands.DynamicsAx
{
    public class OrderAxErrorCommand : ICommand<ValidationResult>
    {
        public OrderAxErrorCommand()
        {
            OrderItems = new List<OrderItemAx>();
        }
        public int OrderNumber { get; set; }
        public string Comment { get; set; }
        public List<OrderItemAx> OrderItems { get; set; }
    }
     public class OrderAxErrorCommandHandler : ICommandHandler<OrderAxErrorCommand, ValidationResult>
     {
         private readonly IRepository<Order, Guid> _repository;
         private readonly ICurrentOrganization _currentOrganization;
         private readonly ICommandBus _commandBus;

         public OrderAxErrorCommandHandler(IRepository<Order, Guid> repository, ICurrentOrganization currentOrganization, ICommandBus commandBus)
         {
             _repository = repository;
             _currentOrganization = currentOrganization;
             _commandBus = commandBus;
         }
         public async Task<ValidationResult> Handle(OrderAxErrorCommand request, CancellationToken cancellationToken)
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
                 order.OrderStatus = OrderStatus.CanceledAx;
                 order.Comment = request.Comment;
                await _commandBus.SendAsync(new CancelPreparationsForOrderCommand { OrderId = order.Id }, cancellationToken);
                _repository.Update(order);
                 await _repository.UnitOfWork.SaveChangesAsync();
               
                 
                 return default;
             }
             catch (Exception ex)
             {
                 var validations = new ValidationResult
                     { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                 return validations;
             }
         }

         private async Task IncreaseQuota(Order order, OrderItem item, CancellationToken cancellationToken)
         {
             await _commandBus.SendAsync(new IncreaseQuotaCommand
             {
                 ProductId = item.ProductId,
                 CustomerId = order.CustomerId,
                 SalesPersonId = order.CreatedByUserId,
                 Quantity = item.Quantity
             }, cancellationToken);
         }
     }
}