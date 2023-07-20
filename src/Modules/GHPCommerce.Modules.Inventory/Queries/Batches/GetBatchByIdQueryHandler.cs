using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Batches.Dtos;
using GHPCommerce.Core.Shared.Contracts.Batches.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.Batches
{
    public class GetBatchByIdQueryHandler: ICommandHandler<GetBatchByIdQuery, BatchDtoV1>
    {
        private readonly IRepository<Batch, Guid> _batchRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetBatchByIdQueryHandler(IRepository<Batch, Guid> batchRepository, ICurrentOrganization currentOrganization,IMapper mapper
        )
        {
            _batchRepository = batchRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }
        public async Task<BatchDtoV1> Handle(GetBatchByIdQuery request, CancellationToken cancellationToken)
        {
            var orgId =  await _currentOrganization.GetCurrentOrganizationIdAsync();
            
            var batch = await _batchRepository.Table.Where(x =>
                    x.InternalBatchNumber == request.InternalBatchNumber &&
                    x.VendorBatchNumber == request.VendorBatchNumber
                    && x.OrganizationId == orgId.Value
                    && x.ProductId == request.ProductId).FirstOrDefaultAsync(cancellationToken);
            return _mapper.Map<BatchDtoV1>(batch);
        }
    }
}