using AutoMapper;
using GHPCommerce.Application.Catalog.Packaging.DTOs;
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

namespace GHPCommerce.Application.Catalog.Packaging.Queries
{
    public class GetPackagingPagedQuery : ICommand<SyncPagedResult<PackagingDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetPackagingPagedQueryHandler : ICommandHandler<GetPackagingPagedQuery, SyncPagedResult<PackagingDto>>
    {
        private readonly IRepository<Domain.Domain.Catalog.Packaging, Guid> _packagingRepository;
        private readonly IMapper _mapper;

        public GetPackagingPagedQueryHandler(IRepository<Domain.Domain.Catalog.Packaging, Guid> packagingRepository, IMapper mapper)
        {
            _packagingRepository = packagingRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<PackagingDto>> Handle(GetPackagingPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _packagingRepository.Table.AsNoTracking()
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Page(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<PackagingDto>>(result);
            return new SyncPagedResult<PackagingDto> { Result = data, Count = total };
        }
    }
}
