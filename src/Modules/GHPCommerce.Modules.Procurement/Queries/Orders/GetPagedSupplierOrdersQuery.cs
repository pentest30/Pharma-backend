using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Queries.Orders
{
    public class GetPagedSupplierOrdersQuery : ICommand<SyncPagedResult<SupplierOrderDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
    public class GetPagedSupplierOrdersQueryHandler: ICommandHandler<GetPagedSupplierOrdersQuery, SyncPagedResult<SupplierOrderDto>>
    {
        private readonly IRepository<SupplierOrder, Guid> _supplierOrderRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetPagedSupplierOrdersQueryHandler(IRepository<SupplierOrder, Guid> supplierOrderRepository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper, ICommandBus commandBus, 
            ICurrentUser currentUser  )
        {
            _supplierOrderRepository = supplierOrderRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }

        public async Task<SyncPagedResult<SupplierOrderDto>> Handle(GetPagedSupplierOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _supplierOrderRepository.Table
                .Include(c => c.OrderItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(x => x.CustomerId == orgId
                && x.OrderStatus != ProcurmentOrderStatus.Removed)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            var result = await query
               // .OrderByDescending(x => x.CreatedDateTime)
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            foreach (var orderOrderItem in result.SelectMany(x => x.OrderItems))
            {
                orderOrderItem.Order = null;
            }
            var dataForSalesPerson = _mapper.Map<List<SupplierOrderDto>>(result);
            foreach (var order in dataForSalesPerson)
            {
                order.OrderId = order.Id;
                order.Type = order.Psychotropic  ? "Psychotrope" : "Non psychotrope";
                order.Status = GetOrderStatus(order.SupplierOrderStatus);
                order.TotalExcludeTax = order.OrderItems.Sum(x => x.Quantity * x.UnitPrice);
                var createUser =  await _commandBus.SendAsync(new GetUserQuery {Id = order.CreatedByUserId},cancellationToken);
                if (createUser != null) order.CreatedBy = createUser.UserName;
                if (order.UpdatedByUserId != null)
                {
                    var updateUser = await _commandBus.SendAsync(new GetUserQuery {Id = order.UpdatedByUserId.Value},cancellationToken);
                    if (updateUser != null) order.UpdatedBy = updateUser.UserName;
                }
            }
            return new SyncPagedResult<SupplierOrderDto> {
                Result = dataForSalesPerson, 
                Count = await query.CountAsync(cancellationToken)
            };
        }

        private string GetOrderStatus(uint status)
        {
            return status switch
            {
                10 => "EN ATTENTE",
                20 => "Enregistré",
                30 => "Acceptée",
                40 => "En cours de traitement",
                50 => "En route",
                60 => "Terminée",
                70 => "Annulée",
                90 => "Confirmée / Imprimée",
                80 => "Rejetée",
                _ => String.Empty
            };
        }
    }
}
