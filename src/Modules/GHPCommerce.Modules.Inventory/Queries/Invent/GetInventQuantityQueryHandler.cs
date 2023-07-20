using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Invent
{
    public class GetInventQuantityQueryHandler : ICommandHandler<GetInventQuantityQuery, double>
    {
        private readonly IRepository<Entities.Invent, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;

        public GetInventQuantityQueryHandler(IRepository<Entities.Invent, Guid> repository, ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
        }
        public async Task<double> Handle(GetInventQuantityQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            return await _repository.Table
                .Where(x => x.ProductId == request.ProductId
                            && x.VendorBatchNumber == request.InternalBatchNumber 
                            &&  x.InternalBatchNumber == request.InternalBatchNumber
                            && x.OrganizationId == orgId.Value)
                .SumAsync(x => x.PhysicalQuantity, cancellationToken: cancellationToken);
        }
    }
}