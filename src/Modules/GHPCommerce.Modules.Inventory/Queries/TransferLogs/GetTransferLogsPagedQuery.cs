using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.DTOs.TransferLogs;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.TransferLogs
{
    public class GetTransferLogsPagedQuery : ICommand<SyncPagedResult<TransferLogDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
    public class GetTransferLogsPagedQueryHandler : ICommandHandler<GetTransferLogsPagedQuery,SyncPagedResult<TransferLogDto>>
    {
        private readonly IRepository<TransferLog, Guid> _transferRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetTransferLogsPagedQueryHandler(IRepository<TransferLog, Guid> transferRepository,  
            ICurrentOrganization currentOrganization,
            IMapper mapper, 
            ICommandBus commandBus)
        {
            _transferRepository = transferRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }
      
        public async Task<SyncPagedResult<TransferLogDto>> Handle(GetTransferLogsPagedQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<TransferLogDto>();
            var query = _transferRepository.Table
                .Include(x=>x.Items)
                .Where(x => x.OrganizationId == org
                            && x.Status !=TransferLogStatus.Removed)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<TransferLogDto>>(result);
            foreach (var transferLogDto in data)
            {
                var createUser =  await _commandBus.SendAsync(new GetUserQuery {Id = transferLogDto.CreatedByUserId},cancellationToken);
                transferLogDto.CreatedBy = createUser.UserName;
            }

            return new SyncPagedResult<TransferLogDto>
                {Result = request.SyncDataGridQuery.Sorted==null? data.OrderByDescending(x=>x.CreatedDateTime).ToList() :data, Count = total};
        }
    }
}