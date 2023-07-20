using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using NLog;

namespace GHPCommerce.Modules.Procurement.Commands.Orders
{
    public class UpdateSupplierOrderItem : CreateSupplierOrderItem
    {
        
    }
    public class UpdateSupplierOrderItemValidator : AbstractValidator<UpdateSupplierOrderItem>
    {
        public UpdateSupplierOrderItemValidator()
        {
            RuleFor(v => v.ProductCode)
                .MaximumLength(200).
                NotEmpty();
            RuleFor(v => v.ProductId)
                .Must(x => x != Guid.Empty);
         
            RuleFor(v => v.OrderId)
                .Must(x => x != Guid.Empty);
            RuleFor(v => v.Quantity)
                .GreaterThan(0);
            //RuleFor(v => v.DocumentRef)
            //    .NotEmpty();
        }
    }
     public  class UpdateSupplierOrderItemHandler :ICommandHandler<UpdateSupplierOrderItem, ValidationResult>
     {
         private readonly IRepository<SupplierOrder, Guid> _supplierOrderRepository;
         private readonly ICurrentOrganization _currentOrganization;
         private readonly ICommandBus _commandBus;
         private readonly ICurrentUser _currentUser;
         private readonly ICache _redisCache;
         private readonly Logger _logger;
         private const string CACHE_KEY = "_supplier-order";
         public UpdateSupplierOrderItemHandler(IRepository<SupplierOrder, Guid> supplierOrderRepository, ICurrentOrganization currentOrganization, ICommandBus commandBus, ICurrentUser currentUser, ICache redisCache, Logger logger)
         {
             _supplierOrderRepository = supplierOrderRepository;
             _currentOrganization = currentOrganization;
             _commandBus = commandBus;
             _currentUser = currentUser;
             _redisCache = redisCache;
             _logger = logger;
         }
         public async Task<ValidationResult> Handle(UpdateSupplierOrderItem request, CancellationToken cancellationToken)
         
         {
             var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
             if (orgId == default) throw new InvalidOperationException("");
             ValidationResult validations = default;
             var validator = new UpdateSupplierOrderItemValidator();
             var validationErrors = await validator.ValidateAsync(request, cancellationToken);
             if (!validationErrors.IsValid)
                 return validationErrors;


             try
             {
                 var key = await GetOrderKey(request.OrderId);
                 var draftOrder =  _redisCache.Get<CachedOrder>(key);
                 if (draftOrder == null)
                     throw new InvalidOperationException("Commande non trouvée");
                 var index = draftOrder.OrderItems.FindIndex(x => x.ProductId == request.ProductId);
                 if (index < 0)
                     throw new InvalidOperationException("Ligne commande non trouvée");
                 var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId }, cancellationToken);

                 draftOrder.OrderItems[index].Discount =(double) request.Discount;
                 draftOrder.OrderItems[index].Quantity =request.Quantity;
                 draftOrder.OrderItems[index].MinExpiryDate = request.MinExpiryDate;
                 draftOrder.OrderItems[index].ExpiryDate = request.ExpiryDate;
                 draftOrder.ExpectedShippingDate = request.ExpectedDeliveryDate;
                 draftOrder.RefDocument = request.DocumentRef;
                 draftOrder.OrderDate = request.OrderDate;
                 draftOrder.UpdatedBy = currentUser.UserName;
                 draftOrder.UpdatedDateTime = DateTimeOffset.Now.Date;
                 await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
             }
             catch (Exception ex)
             {
                 validations = new ValidationResult
                     {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                 _logger.Error(ex.Message);
                 _logger.Error(ex.InnerException?.Message);
             }

             return validations;

         }
         private async Task<string> GetOrderKey( Guid orderId)
         {
             var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();

             return orgId.ToString() + _currentUser.UserId + orderId;
         }
     }
}