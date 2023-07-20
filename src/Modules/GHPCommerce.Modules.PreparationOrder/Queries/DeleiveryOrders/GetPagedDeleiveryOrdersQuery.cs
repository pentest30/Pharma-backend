using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.DTOs.DeleiveryOrders;
using GHPCommerce.Modules.PreparationOrder.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.PreparationOrder.Queries.DeleiveryOrders
{
    public class GetPagedDeleiveryOrdersQuery: ICommand<SyncPagedResult<DeleiveryOrderDto>>
    {
        public string barCode { get; set; }
        public SyncDataGridQuery DataGridQuery { get; set; }

    }
    public class GetPagedDeleiveryOrdersQueryHandler : ICommandHandler<GetPagedDeleiveryOrdersQuery, SyncPagedResult<DeleiveryOrderDto>>
    {
        private readonly IRepository<DeleiveryOrder, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;

        public GetPagedDeleiveryOrdersQueryHandler(IRepository<DeleiveryOrder, Guid> repository,  
            ICurrentOrganization currentOrganization, 
            IMapper mapper)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<DeleiveryOrderDto>> Handle(GetPagedDeleiveryOrdersQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _repository.Table
                .Include(c => c.DeleiveryOrderItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(x => x.OrganizationId == orgId && (String.IsNullOrEmpty(request.barCode) || x.OrderIdentifier == request.barCode))
                .DynamicWhereQuery(request.DataGridQuery);
            var result = await query
                .OrderByDescending(x => x.CreatedDateTime)
                .Page(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1,
                    request.DataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<DeleiveryOrderDto>>(result);
            foreach (var item in data)
            {
                item.deleiveryOrderId = item.Id;
            }
            return new SyncPagedResult<DeleiveryOrderDto>
            {
                Result = data,
                Count = await query.CountAsync(cancellationToken)
            };
        }
    }
}