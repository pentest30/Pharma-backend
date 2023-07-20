using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Batches.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Batches
{
    public class GetBatchIdQueryHandler : ICommandHandler<GetBatchIdQuery, Guid>
    {
        private readonly IRepository<Batch, Guid> _batchRepository;
        private readonly ICurrentOrganization _currentOrganization;

        public GetBatchIdQueryHandler(IRepository<Batch, Guid> batchRepository, ICurrentOrganization currentOrganization)
        {
            _batchRepository = batchRepository;
            _currentOrganization = currentOrganization;
        }
        public async Task<Guid> Handle(GetBatchIdQuery request, CancellationToken cancellationToken)
        {
            var orgId =  await _currentOrganization.GetCurrentOrganizationIdAsync();
            var batch = await _batchRepository.Table.Where(x =>
                x.InternalBatchNumber == request.InternalBatchNumber &&
                x.VendorBatchNumber == request.VendorBatchNumber
                && x.OrganizationId == orgId.Value
                && x.ProductId == request.ProductId)
                .Select(x=> x.Id).FirstOrDefaultAsync(cancellationToken);
            return batch;
        }
    }
}