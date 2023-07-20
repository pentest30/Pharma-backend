using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AutoMapper.QueryableExtensions;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Modules.Inventory.DTOs.Invent;

namespace GHPCommerce.Modules.Inventory.Queries.Invent
{
    public class GetInventsPagedQuery: ICommand<SyncPagedResult<InventDtoV1>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }

    public class GetInventsPagedQueryHandler : ICommandHandler<GetInventsPagedQuery, SyncPagedResult<InventDtoV1>>
    {
        private readonly IRepository<Entities.Invent, Guid> _inventRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetInventsPagedQueryHandler(IRepository<Entities.Invent, Guid> inventRepository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            ICommandBus commandBus)
        {
            _inventRepository = inventRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }
        public async Task<SyncPagedResult<InventDtoV1>> Handle(GetInventsPagedQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<InventDtoV1>();
            var query = _inventRepository.Table
                .Where(x => x.OrganizationId == org)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<InventDtoV1>>(result);
            foreach (var inventDto in data)
            {
                var createUser =  await _commandBus.SendAsync(new GetUserQuery {Id = inventDto.CreatedByUserId},cancellationToken);
                inventDto.CreatedBy = createUser.UserName;
            }

            return new SyncPagedResult<InventDtoV1>
                {Result = data, Count = total};
        }
        
    }
}