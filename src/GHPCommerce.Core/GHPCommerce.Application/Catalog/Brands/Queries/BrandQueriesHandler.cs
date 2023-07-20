using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Brands.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace GHPCommerce.Application.Catalog.Brands.Queries
{
    public class BrandQueriesHandler :
        ICommandHandler<GetBrandsListQuery, PagingResult<BrandDto>>,
        ICommandHandler<GetBrandByIdQuery, BrandDto>,
        ICommandHandler<GetAllBrandsQuery, IEnumerable<BrandDto>>
    {
        private readonly IRepository<Brand, Guid> _brandRepository;
        private readonly IMapper _mapper;
        public BrandQueriesHandler(IMapper mapper, IRepository<Brand, Guid> brandRepository)
        {
            _mapper = mapper;
            _brandRepository = brandRepository;
        }
        public async Task<PagingResult<BrandDto>> Handle(GetBrandsListQuery request, CancellationToken cancellationToken)
        {
            var total = await _brandRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _brandRepository
                .Table
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<IEnumerable<BrandDto>>(query);
            return new PagingResult<BrandDto> { Data = data, Total = total };

        }

        public async Task<BrandDto> Handle(GetBrandByIdQuery request, CancellationToken cancellationToken)
        {
            var brand = await _brandRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (brand == null)
                throw new NotFoundException($"Brand with id: {request.Id} was not found");
            return _mapper.Map<BrandDto>(brand);
        }

        public async Task<IEnumerable<BrandDto>> Handle(GetAllBrandsQuery request, CancellationToken cancellationToken)
        {
            var query = await _brandRepository
                .Table
                
                .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<BrandDto>>(query);
        }

      
    }
}
