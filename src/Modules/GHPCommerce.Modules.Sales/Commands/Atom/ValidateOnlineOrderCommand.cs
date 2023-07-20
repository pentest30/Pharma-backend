using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Core.Shared.Enums;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Commands.Orders;
using NLog;

namespace GHPCommerce.Modules.Sales.Commands.Atom
{
    public class ValidateOnlineOrderCommand : ICommand<ValidationResult>
    {
        public string CustomerCode { get; set; }
        public Guid OrderId { get; set; }
    }
    public  class  ValidateOnlineOrderCommandHandler : ICommandHandler<ValidateOnlineOrderCommand, ValidationResult>
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _cache;
        private readonly Logger _logger;

        public ValidateOnlineOrderCommandHandler(ICommandBus commandBus, 
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser, 
            ICache cache,
            Logger log)
        {
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _cache = cache;
            _logger = log;
        }
        public async Task<ValidationResult> Handle(ValidateOnlineOrderCommand request, CancellationToken cancellationToken) 
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            try
            {
                if (orgId == null)
                {
                    throw new InvalidOperationException("Organization non valide");
                }
                var activeCustomer = await _commandBus.SendAsync(new GetCustomerByCodeQueryV2
                {
                    Code = request.CustomerCode, 
                    SupplierOrganizationId = orgId.Value
                }, cancellationToken);
                if (activeCustomer?.CustomerId == null)
                    throw new InvalidOperationException("Client non valide");
                if (activeCustomer.CustomerState == CustomerState.Blocked)
                    throw new InvalidOperationException("Client bloqué");
                var userId = activeCustomer.SalesPersonId ?? _currentUser.UserId;
                var key = activeCustomer.CustomerId + userId.ToString() + request.OrderId;
                var draftOrder = await _cache.GetAsync<CachedOrder>(key, cancellationToken);
                if (draftOrder == null)
                {
                    throw new InvalidOperationException("Commande validée ou supprimée");
                }
                var r = await SaveOnlineOrderAsync(draftOrder, userId, cancellationToken);
                if (r != null && !r.IsValid)
                {
                    return r;
                }
            }
            catch (Exception ex)
            {
               var  validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                _logger.Error(nameof(ValidateOnlineOrderCommand)+": "+   ex.Message);
                _logger.Error(ex.InnerException?.Message);
                return validations;
            }

            return default;
        }
        private async Task<ValidationResult> SaveOnlineOrderAsync(CachedOrder draftOrder , Guid userId, CancellationToken cancellationToken)
        {
            var saveOrderCmd = new SendOrderByPharmacistCommand
            {
                Id = draftOrder.Id,
                SupplierId = draftOrder.SupplierId,
                OnlineOrder = true,
                OrderType = !draftOrder.Psychotropic ? OrderType.NonPsychotrope : OrderType.Psychotrope,
                RefDocument = draftOrder.Psychotropic ? draftOrder.RefDocumentHpcs : String.Empty,
                CustomerId = draftOrder.CustomerId,
                ExpectedShippingDate = DateTime.Now,
                DefaultSalesPerson = userId
            };
           return  await _commandBus.SendAsync(saveOrderCmd, cancellationToken);
        }
    }
}