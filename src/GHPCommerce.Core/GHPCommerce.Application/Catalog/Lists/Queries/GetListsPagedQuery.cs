using AutoMapper;
using GHPCommerce.Application.Catalog.Lists.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Application.Catalog.Lists.Queries
{
    public class GetListsPagedQuery : ICommand<SyncPagedResult<ListDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetListsPagedQueryHandler : ICommandHandler<GetListsPagedQuery, SyncPagedResult<ListDto>>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IMapper _mapper;

        public GetListsPagedQueryHandler(IRepository<List, Guid> listRepository, IMapper mapper)
        {
            _listRepository = listRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<ListDto>> Handle(GetListsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _listRepository.Table.AsNoTracking()
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Page(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<ListDto>>(result);
            return new SyncPagedResult<ListDto> { Result = data, Count = total };
        }
    }
   
}
