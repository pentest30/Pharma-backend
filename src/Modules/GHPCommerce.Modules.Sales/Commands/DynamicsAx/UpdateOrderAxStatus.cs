using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Quota;
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
    public class UpdateOrderAxStatus : ICommand<ValidationResult>
    {
      
        public string CodeAx { get; set; }
        public int OrderNumber { get; set; }
        public OrderAxStatus OrderAxStatus { get; set; }
       
    }
     public class UpdateOrderAxStatusHandler : ICommandHandler<UpdateOrderAxStatus, ValidationResult>
     {
         private readonly IRepository<Order, Guid> _repository;
         private readonly ICurrentOrganization _currentOrganization;
         private readonly ICache _cache;
         private readonly Logger _logger;
         private readonly ICommandBus _commandBus;
         private readonly MedIJKModel _interfacing;

         public UpdateOrderAxStatusHandler(IRepository<Order, Guid> repository, 
             ICurrentOrganization currentOrganization, 
             ICache cache,
             Logger logger, 
             ICommandBus commandBus,
             MedIJKModel interfacing)
         {
             _repository = repository;
             _currentOrganization = currentOrganization;
             _cache = cache;
             _logger = logger;
             _commandBus = commandBus;
             _interfacing = interfacing;
         }
         public async Task<ValidationResult> Handle(UpdateOrderAxStatus request, CancellationToken cancellationToken)
         {
             try
             {
                 var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                 if (!orgId.HasValue)
                 {
                     throw new InvalidOperationException("Resources not allowed");
                 }

                 if (string.IsNullOrEmpty(request.CodeAx))
                 {
                     throw new InvalidOperationException("Order number is empty");
                 }

                 var order = await _repository.Table
                     .Include(x => x.OrderItems)
                     .FirstOrDefaultAsync(x => x.CodeAx == request.CodeAx
                                               && x.SupplierId == orgId.Value, cancellationToken: cancellationToken);
                 if (order == default)
                 {
                     throw new NotFoundException($"Order with number {request.CodeAx} was not found");
                 }

                 var oldStatus = order.OrderStatus;
                 order.OrderStatus = MapOrderStatus(request.OrderAxStatus);
                 _repository.Update(order);
                 await _repository.UnitOfWork.SaveChangesAsync();
                 if (order.OrderStatus != OrderStatus.CanceledAx || oldStatus == OrderStatus.Canceled) return default;
                 await _commandBus.SendAsync(new CancelOrderCommand { Id = order.Id }, cancellationToken);
                 return default;
             }
             catch (Exception ex)
             {
                 var validations = new ValidationResult
                     { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                 _logger.Error(nameof(UpdateOrderAxStatus) + ": " + ex.Message);
                 _logger.Error(ex.InnerException?.Message);
                 return validations;
             }
         }
         private async Task IncreaseQuota(Guid customerId, Guid salesPersonId, OrderItem item, CancellationToken cancellationToken)
         {
             await _commandBus.SendAsync(new IncreaseQuotaCommand
             {
                 ProductId = item.ProductId,
                 CustomerId = customerId,
                 SalesPersonId = salesPersonId,
                 Quantity = item.Quantity
             }, cancellationToken);
         }

         private OrderStatus MapOrderStatus(OrderAxStatus status)
         {
             switch (status)
             {
                 case OrderAxStatus.Pending: return OrderStatus.Pending;
                 case OrderAxStatus.Canceled: return OrderStatus.CanceledAx;
                 case OrderAxStatus.Shipped: return OrderStatus.Shipped;
                 case OrderAxStatus.InShippingZone: return OrderStatus.InShippingArea;
                 case OrderAxStatus.Loading: return OrderStatus.Loading;
                 case OrderAxStatus.Invoiced: return OrderStatus.Invoiced;
                 case OrderAxStatus.Withdrawn: return OrderStatus.Withdrawn;
                 case OrderAxStatus.BeingWithdrawn: return OrderStatus.BeingWithdrawn;
                 case OrderAxStatus.AcknowledgmentOfrReceipt: return OrderStatus.AcknowledgmentOfrReceipt;
                 case OrderAxStatus.AxError: return OrderStatus.AxError;
                 case OrderAxStatus.Inprogress: return OrderStatus.Processing;
                 case OrderAxStatus.Confirmed: return OrderStatus.Accepted;
             }

             return default;
         }
        
     }



     public enum OrderAxStatus : uint
     {
         // en instance
         Pending = 0,

         //En cours de prélèvement
         BeingWithdrawn = 1,

         //Prélevé
         Withdrawn = 2,

         //Facturé
         Invoiced = 3,

         // En Zone d'expédition
         InShippingZone = 4,

         //En cours de chargement
         Loading = 5,

         //Expédié
         Shipped = 6,

         //Annulée
         Canceled = 7,

         //Accusé de réception
         AcknowledgmentOfrReceipt = 8,
         AxError = 9,

         //En cours 
         Inprogress = 101,
         // confirmé
         Confirmed = 102,

     }
}