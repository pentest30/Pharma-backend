using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs.Batches;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;

namespace GHPCommerce.Modules.Inventory.Queries.Batches
{
    public class GetBatchPagedQuery : ICommand<SyncPagedResult<BatchDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }

    public class GetBatchPagedQueryHandler : ICommandHandler<GetBatchPagedQuery, SyncPagedResult<BatchDto>>
    {
        private readonly IRepository<Batch, Guid> _batchRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetBatchPagedQueryHandler(IRepository<Batch, Guid> batchRepository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            ICommandBus commandBus)
        {
            _batchRepository = batchRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }
        public async Task<SyncPagedResult<BatchDto>> Handle(GetBatchPagedQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<BatchDto>();
            var query = _batchRepository.Table
                .Where(x => x.OrganizationId == org)
                .DynamicWhereQuery(request.SyncDataGridQuery);
           
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
               // .OrderByDescending(x => x.CreatedDateTime)
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<BatchDto>>(result);
            foreach (var batchDto in data)
            {
                var createUser =  await _commandBus.SendAsync(new GetUserQuery {Id = batchDto.CreatedByUserId},cancellationToken);
                batchDto.CreatedBy = createUser.UserName;

            }

            return new SyncPagedResult<BatchDto>
                {Result = data, Count = total};
        }
    }
}