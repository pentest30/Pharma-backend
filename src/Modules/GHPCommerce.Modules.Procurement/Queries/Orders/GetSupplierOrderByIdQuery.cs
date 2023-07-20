using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.DTOs;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Procurement.Queries.Orders
{
    public class GetSupplierOrderByIdQuery : ICommand<SupplierOrderDto>
    {
        public Guid OrderId { get; set; }
    }
    public class GetSupplierOrderByIdQueryHandler : ICommandHandler<GetSupplierOrderByIdQuery, SupplierOrderDto>
    {
        private readonly IRepository<SupplierOrder, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetSupplierOrderByIdQueryHandler(IRepository<SupplierOrder, Guid> repository,
            ICurrentOrganization currentOrganization,
            IMapper mapper)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }
        public async Task<SupplierOrderDto> Handle(GetSupplierOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = await _repository.Table.Include(c => c.OrderItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .FirstOrDefaultAsync(x => x.CustomerId == orgId 
                                          && request.OrderId == x.Id
                    && x.OrderStatus != ProcurmentOrderStatus.Removed, cancellationToken: cancellationToken);
            return _mapper.Map<SupplierOrderDto>(query);

        }
    }
}
