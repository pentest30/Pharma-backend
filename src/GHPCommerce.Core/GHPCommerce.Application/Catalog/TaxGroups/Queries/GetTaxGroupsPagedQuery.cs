using AutoMapper;
using GHPCommerce.Application.Catalog.TaxGroups.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Application.Catalog.TaxGroups.Queries
{
    public class GetTaxGroupsPagedQuery : ICommand<SyncPagedResult<TaxGroupDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetTaxGroupsPagedQueryHandler : ICommandHandler<GetTaxGroupsPagedQuery, SyncPagedResult<TaxGroupDto>>
    {
        private readonly IRepository<TaxGroup, Guid> _taxRepository;
        private readonly IMapper _mapper;

        public GetTaxGroupsPagedQueryHandler(IRepository<TaxGroup, Guid> taxGroupRepository, IMapper mapper)
        {
            _taxRepository = taxGroupRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<TaxGroupDto>> Handle(GetTaxGroupsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _taxRepository.Table.AsNoTracking()
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Page(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<TaxGroupDto>>(result);
            return new SyncPagedResult<TaxGroupDto> { Result = data, Count = total };
        }
    }
}
