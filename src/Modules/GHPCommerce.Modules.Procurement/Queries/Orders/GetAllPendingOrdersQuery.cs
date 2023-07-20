using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.DTOs;

namespace GHPCommerce.Modules.Procurement.Queries.Orders
{
    public class GetAllPendingOrdersQuery : ICommand<IEnumerable<SupplierOrderDto>>
    {
    }
    public class  GetAllPendingOrdersQueryHandler : ICommandHandler<GetAllPendingOrdersQuery, IEnumerable<SupplierOrderDto>>
    {
        private readonly ICache _redisCache;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;
        private const string CACHE_KEY = "_supplier-order";
        public GetAllPendingOrdersQueryHandler(ICache redisCache, 
            ICurrentOrganization currentOrganization, 
            ICurrentUser currentUser, IMapper mapper)
        {
            _redisCache = redisCache;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _mapper = mapper;
        }
        public async Task<IEnumerable<SupplierOrderDto>> Handle(GetAllPendingOrdersQuery request, CancellationToken cancellationToken)
        {
            var lookupKey = await GetLookupKey();
            var lookUp = _redisCache.Get<List<Guid>>(lookupKey);
            if (lookUp == null || !lookUp.Any())
                return new List<SupplierOrderDto>();

            var result = new List<SupplierOrderDto>();
            foreach (var lookupOrder in lookUp)
            {
                var secondKey =  await GetOrderKey( lookupOrder);
                var draftOrder =  _redisCache.Get<CachedOrder>(secondKey);
                if (draftOrder == null) continue;
               
                var order = GetOrderDto(draftOrder);
                order.OrderTotal = order.OrderItems.Sum(x => x.Quantity * x.UnitPrice);
              
                result.Add(order);
            }

            return result;
        }
          private SupplierOrderDto GetOrderDto(CachedOrder draftOrder)
        {
            var order = new SupplierOrderDto
            {
                Id = draftOrder.Id,
                OrderId = draftOrder.Id,
                SupplierId = draftOrder.SupplierId,
                CustomerId = draftOrder.CustomerId,
                OrderNumber = draftOrder.OrderNumber,
                OrderDate = draftOrder.OrderDate,
                CreatedBy = draftOrder.CreatedBy,
                UpdatedBy = draftOrder.UpdatedBy,
                Psychotropic = draftOrder.Psychotropic,
                ExpectedShippingDate = draftOrder.ExpectedShippingDate,
                OrderItems = _mapper.Map<List<SupplierOrderItemDto>>(draftOrder.OrderItems)
                ,RefDocument = draftOrder.RefDocument
            };
            
            return _mapper.Map<SupplierOrderDto>(order);
        }

        private async Task<string> GetLookupKey()
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            return orgId.ToString() + _currentUser.UserId + CACHE_KEY;
        }
        private async Task<string> GetOrderKey( Guid orderId)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();

            return orgId.ToString() + _currentUser.UserId + orderId;
        }
    }
}