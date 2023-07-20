using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
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
    public class CreateSupplierOrderItem : ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
       
        public int Quantity { get; set; }
        public int OldQuantity { get; set; }
        public Guid SupplierOrganizationId { get; set; }
        public DateTime? MinExpiryDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string InternalBatchNumber { get; set; }
        public string ProductCode { get; set; }
        public bool Psychotropic { get; set; }
        public decimal ExtraDiscount { get; set; }
        public decimal Discount { get; set; }
        public string DocumentRef { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public string VendorBatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        
    }
    public class CreateSupplierOrderItemValidator : AbstractValidator<CreateSupplierOrderItem>
    {
        public CreateSupplierOrderItemValidator()
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
       
            
        }
    }
    public class CreateSupplierOrderItemHandler : ICommandHandler<CreateSupplierOrderItem, ValidationResult>
    {
        private readonly IRepository<SupplierOrder, Guid> _supplierOrderRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;
        private readonly Logger _logger;
        private const string CACHE_KEY = "_supplier-order";
        public CreateSupplierOrderItemHandler(IRepository<SupplierOrder , Guid> supplierOrderRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser,
            ICache redisCache,
             Logger logger)
        {
            _supplierOrderRepository = supplierOrderRepository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(CreateSupplierOrderItem request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException("");
            ValidationResult validations = default;
            var validator = new CreateSupplierOrderItemValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            if (!validationErrors.IsValid)
                return validationErrors;

            CachedOrder? draftOrder = default;
            CachedOrderItem orderItem = default;
            bool reserved = false;
            var lookupKey = await GetLookupKey();
            string key = await GetOrderKey(request.OrderId);
            var lookUp = _redisCache.Get<List<Guid>>(lookupKey);

            if (lookUp == null)
                await _redisCache.AddOrUpdateAsync<List<Guid>>(lookupKey, new List<Guid> {request.OrderId}, cancellationToken);
            else if (lookUp.All(x => x != request.OrderId))
            {
                lookUp.Add(request.OrderId);
                await _redisCache.AddOrUpdateAsync<List<Guid>>(lookupKey, lookUp, cancellationToken);
            }
          
            try
            {
                draftOrder =  await GetCurrentPendingOrder(key, 
                    request.OrderId,
                    request.SupplierOrganizationId,
                    docRef: request.DocumentRef,
                    psychotropic: request.Psychotropic, 
                    orderDate:request.OrderDate == default? DateTime.Now .Date : request.OrderDate.Date,
                    supplierName: request.SupplierName, 
                    customerName: request.CustomerName);
                if (draftOrder == null)
                    throw new InvalidOperationException("Commande non trouvée");
                var currentUser =
                    await _commandBus.SendAsync(new GetUserQuery {Id = _currentUser.UserId, IncludeRoles = true},
                        cancellationToken);

                if (string.IsNullOrEmpty(draftOrder.CreatedBy))
                {
                    draftOrder.CreatedBy = currentUser.UserName;
                    draftOrder.CreatedDateTime = DateTimeOffset.Now;
                }
                draftOrder.ExpectedShippingDate = request.ExpectedDeliveryDate;
              //  draftOrder.OrderItems = new List<CachedOrderItem>();
                draftOrder.OrderItems .Add(await AddOrderItem(request));
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
        private async Task<CachedOrder> GetCurrentPendingOrder(string key, 
            Guid orderId, 
            Guid? supplierOrganizationId = null,
            bool create = true,
            string docRef = "", 
            bool? psychotropic = null, 
            DateTime? orderDate = null ,
            string supplierName  = null!,
            string customerName = null!)
        {
            //if create=false, we just return the existing Order
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null) throw new InvalidOperationException();

            var draftOrder = _redisCache.Get<CachedOrder>(key);
            if (draftOrder == null)
            {
                if (!create) return null;
                if (supplierOrganizationId != null)
                {
                    draftOrder = new CachedOrder
                    {
                        Id = orderId,
                        SupplierId = supplierOrganizationId.Value,
                        CreatedByUserId = _currentUser.UserId,
                        OrderNumber = "BC-" + long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")),
                        OrderDate = orderDate ?? DateTime.Now,
                        RefDocument = docRef, 
                        CustomerId = orgId.Value,
                        CustomerName =  customerName,
                        SupplierName = supplierName

                    };
                    if (psychotropic.HasValue)
                        draftOrder.Psychotropic = psychotropic.Value;
                    await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder);
                }
            }
            

            return draftOrder;

        }
        private async Task<string> GetOrderKey( Guid orderId)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();

            return orgId.ToString() + _currentUser.UserId + orderId;
        }

        private async Task<string> GetLookupKey()
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();

            return orgId.ToString() + _currentUser.UserId + CACHE_KEY;
        }
        private async  Task<CachedOrderItem>  AddOrderItem(CreateSupplierOrderItem request)
        {
            var product = await _commandBus.SendAsync(new GetProductById { Id = request.ProductId });
            CachedOrderItem orderItem = new CachedOrderItem();
            orderItem.Id = Guid.NewGuid();
            orderItem.Quantity = request.Quantity;
            orderItem.OrderId = request.OrderId;
            orderItem.ProductName = request.ProductName;
            orderItem.ProductId = request.ProductId;
            orderItem.VendorBatchNumber = request.VendorBatchNumber;
            orderItem.ProductCode = request.ProductCode;
            orderItem.ExtraDiscount = request.ExtraDiscount / 100;
            orderItem.UnitPrice = request.UnitPrice == 0 ? product.UnitPrice: request.UnitPrice;
            orderItem.ExpiryDate = request.ExpiryDate;
            orderItem.Discount =(double) request.Discount;
            orderItem.Tax =(double) product.Tax;
            orderItem.MinExpiryDate = request.MinExpiryDate;

            return orderItem;
            
        }
    }
}