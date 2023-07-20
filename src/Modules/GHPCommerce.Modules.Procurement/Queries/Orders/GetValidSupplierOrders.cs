using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Queries.Orders
{
    public class GetValidSupplierOrders : ICommand<IEnumerable<SupplierOrderDto>>
    {
        public Guid SupplierId { get; set; }
    }
    public  class GetValidSupplierOrdersHandler : ICommandHandler<GetValidSupplierOrders, IEnumerable<SupplierOrderDto>>
    {
        private readonly IRepository<SupplierOrder, Guid> _supplierOrderRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetValidSupplierOrdersHandler(IRepository<SupplierOrder, Guid> repository,
            ICurrentOrganization currentOrganization,
            IMapper mapper)
        {
            _supplierOrderRepository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }
        public async Task<IEnumerable<SupplierOrderDto>> Handle(GetValidSupplierOrders request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query =await _supplierOrderRepository
                .Table
                .Include(x=>x.OrderItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(x => x.CustomerId == orgId && x.SupplierId == request.SupplierId && x.OrderStatus == ProcurmentOrderStatus.Processing) 
                .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<List<SupplierOrderDto>>(query);
        }
    }
}