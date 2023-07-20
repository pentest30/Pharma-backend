using AutoMapper;
using GHPCommerce.Application.Catalog.INNs.DTOs;
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

namespace GHPCommerce.Application.Catalog.INNs.Queries
{
    public class GetInnPagedQuery : ICommand<SyncPagedResult<InnDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetInnPagedQueryHandler : ICommandHandler<GetInnPagedQuery, SyncPagedResult<InnDto>>
    {
        private readonly IRepository<INN, Guid> _taxRepository;
        private readonly IMapper _mapper;

        public GetInnPagedQueryHandler(IRepository<INN, Guid> INNRepository, IMapper mapper)
        {
            _taxRepository = INNRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<InnDto>> Handle(GetInnPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _taxRepository.Table.AsNoTracking()
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Page(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<InnDto>>(result);
            return new SyncPagedResult<InnDto> { Result = data, Count = total };
        }
    }
}
