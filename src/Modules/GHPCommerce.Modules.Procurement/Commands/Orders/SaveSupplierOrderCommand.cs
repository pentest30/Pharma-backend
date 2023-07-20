using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using GHPCommerce.Modules.Procurement.Repositories;
using Microsoft.EntityFrameworkCore;
using NLog;
using  System.Linq;
using GHPCommerce.Core.Shared.Services;
using GHPCommerce.Infra.OS;

namespace GHPCommerce.Modules.Procurement.Commands.Orders
{
    public class SaveSupplierOrderCommand :ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
    }

    public class SaveSupplierOrderCommandHandler : ICommandHandler<SaveSupplierOrderCommand, ValidationResult>
    {
        private readonly ISupplierOrderRepository _supplierOrderRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ICache _redisCache;
        private readonly ICacheService _cacheService;
        private readonly ISequenceNumberService<SupplierOrder, Guid> _sequenceNumberService;
        private readonly Logger _logger;
        private const string CACHE_KEY = "_supplier-order";
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        public SaveSupplierOrderCommandHandler(ISupplierOrderRepository supplierOrderRepository,
            IMapper mapper,
            ICurrentOrganization currentOrganization,
            ICommandBus commandBus,
            ICurrentUser currentUser,
            ICache redisCache,
            ICacheService cacheService,
            ISequenceNumberService<SupplierOrder, Guid> sequenceNumberService,
            Logger logger)
        {
            _supplierOrderRepository = supplierOrderRepository;
            _mapper = mapper;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _redisCache = redisCache;
            _cacheService = cacheService;
            _sequenceNumberService = sequenceNumberService;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(SaveSupplierOrderCommand request, CancellationToken cancellationToken)
        {
            
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken);
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("");
                string key = await GetOrderKey(request.OrderId);
                var draftOrder = _redisCache.Get<CachedOrder>(key);
                if (draftOrder == null || !draftOrder.OrderItems.Any())
                    throw new InvalidOperationException("Commande non trouvée");
                var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId }, cancellationToken);
                //draftOrder.ExpectedShippingDate = request.ExpectedDeliveryDate;
                draftOrder.UpdatedBy = currentUser.UserName;
                draftOrder.UpdatedDateTime = DateTimeOffset.Now.Date;
                draftOrder.UpdatedByUserId = _currentUser.UserId;
                SupplierOrder? order = await _supplierOrderRepository.Table
                    .Where(x => x.Id == request.OrderId)
                    .Include(x=>x.OrderItems)
                    // .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);
            
                bool newOrder = order == null;
                //
                if (order != null)
                {
                    order.OrderItems.Clear();
                    order.OrderItems.AddRange(_mapper.Map<List<SupplierOrderItem>>(draftOrder.OrderItems));    
                }
                else
                {
                    order = _mapper.Map<SupplierOrder>(draftOrder);
                    order.OrganizationId = orgId.Value;
                    var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(order.OrderDate, orgId.Value);
                     order.SequenceNumber = sq ;

                }
                
                order.OrderStatus = ProcurmentOrderStatus.Saved;
                order.ExpectedDeliveryDate = draftOrder.ExpectedShippingDate;
                foreach (var item in order.OrderItems)
                {
                    item.RemainingQuantity = item.Quantity;
                    item.Id = Guid.Empty;
                }
                    
                if(newOrder)_supplierOrderRepository.Add(order);
                else _supplierOrderRepository.Update(order);
                await _supplierOrderRepository.UnitOfWork.SaveChangesAsync();
                _semaphoreSlim.Release();

            }
            catch (Exception ex)
            {
                _semaphoreSlim.Release();
                var validations = new ValidationResult {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                return validations;
              
            }

            return default;
        }
        private async Task<string> GetOrderKey(Guid orderId)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            return orgId.ToString() + _currentUser.UserId + orderId;
        }

        private async Task<string> GetLookupKey()
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            return orgId.ToString() + _currentUser.UserId + CACHE_KEY;
        }
    }
}
