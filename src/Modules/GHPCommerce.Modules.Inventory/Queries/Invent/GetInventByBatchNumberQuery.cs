using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Invent
{
    
    public class GetInventByBatchNumberQueryHandler : ICommandHandler<GetInventByBatchNumberQuery, InventDto>
    {
        private readonly IRepository<Entities.Invent, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetInventByBatchNumberQueryHandler(IRepository<Entities.Invent, Guid> repository, ICurrentOrganization currentOrganization, IMapper mapper)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;

        }
        public async Task<InventDto> Handle(GetInventByBatchNumberQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var originQuantity =  await _repository.Table
                .Where(x => x.ProductId == request.ProductId
                            && x.VendorBatchNumber == request.VendorBatchNumber 
                            &&  x.InternalBatchNumber == request.InternalBatchNumber
                            && x.OrganizationId == orgId.Value)
                .SumAsync(x => x.PhysicalQuantity, cancellationToken: cancellationToken);
            // Get invent from vendable zone Id et stock state liberé
            var result = await _repository.Table
                .Where(x => x.ProductId == request.ProductId
                            && x.VendorBatchNumber == request.VendorBatchNumber 
                            &&  x.InternalBatchNumber == request.InternalBatchNumber
                            && x.OrganizationId == orgId.Value 
                            && x.ZoneId == Guid.Parse("7BD42E21-E657-4F99-AFEF-1AFE5CEACB16")
                            && x.StockStateId == Guid.Parse("7BD32E21-E657-4F99-AFEF-1AFE5CEACB16") )
                .FirstOrDefaultAsync();
            var invent =  _mapper.Map<InventDto>(result);
            invent.originQuantity = originQuantity;
            return invent;
        }
    }
}