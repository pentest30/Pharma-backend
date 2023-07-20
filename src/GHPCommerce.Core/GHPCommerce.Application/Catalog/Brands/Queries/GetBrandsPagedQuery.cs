using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GHPCommerce.Application.Catalog.Brands.DTOs;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Application.Catalog.Brands.Queries
{
    public  class GetBrandsPagedQuery : ICommand<SyncPagedResult<BrandDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; } 
    }
    public class GetBrandsPagedQueryHandler : ICommandHandler<GetBrandsPagedQuery, SyncPagedResult<BrandDto>>
    {
        private readonly IRepository<Brand, Guid> _brandRepository;
        private readonly IMapper _mapper;

        public GetBrandsPagedQueryHandler(IRepository<Brand, Guid> brandRepository, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<BrandDto>> Handle(GetBrandsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _brandRepository.Table
                
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = query
                .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take);
                //.ToListAsync(cancellationToken: cancellationToken);
            var data = await _mapper.ProjectTo<BrandDto>(result).ToListAsync(cancellationToken: cancellationToken);
            return new SyncPagedResult<BrandDto>
                {Result = data, Count = total};
        }
    }
}