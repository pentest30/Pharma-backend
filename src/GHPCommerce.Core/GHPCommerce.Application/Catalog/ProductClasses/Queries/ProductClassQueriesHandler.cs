using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.ProductClasses.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.ProductClasses.Queries
{
    public class ProductClassQueriesHandler :
        ICommandHandler<GetProductClassesListQuery, PagingResult<ProductClassDto>>,
        ICommandHandler<GetProductClassByIdQuery, ProductClassDto>,
        ICommandHandler<GetAllProductClassesQuery, IEnumerable<ProductClassDto>>,
        ICommandHandler<GetCatalogsForCustomerQuery, IEnumerable<CatalogDto>>
    {
        private readonly IRepository<ProductClass, Guid> _productClassRepository;
        private readonly IMapper _mapper;

        public ProductClassQueriesHandler(IRepository<ProductClass, Guid> productClassRepository, IMapper mapper)
        {
            _productClassRepository = productClassRepository;
            _mapper = mapper;
        }
        public async Task<PagingResult<ProductClassDto>> Handle(GetProductClassesListQuery request, CancellationToken cancellationToken)
        {
            var total = await _productClassRepository
                .Table.CountAsync(cancellationToken: cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir: request.SortProp + " " + request.SortDir;
            var query = await _productClassRepository
                .Table
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data = query.Select(productClass => _mapper.Map<ProductClassDto>(productClass)).ToList();
            _mapper.Map<List<ProductClassDto>>(query);
            
            return new PagingResult<ProductClassDto> { Data = data, Total = total };

        }

        public async Task<ProductClassDto> Handle(GetProductClassByIdQuery request, CancellationToken cancellationToken)
        {
            var productClass = await _productClassRepository
                .Table
                .FirstOrDefaultAsync(x=>x.Id ==request.Id,cancellationToken: cancellationToken);
           if(productClass == null)
               throw new NotFoundException($"Product class with id: {request.Id} was not found");
            return _mapper.Map<ProductClassDto>(productClass);
        }

        public async Task<IEnumerable<ProductClassDto>> Handle(GetAllProductClassesQuery request, CancellationToken cancellationToken)
        {
            var query = await _productClassRepository
                .Table
                .Select(x=> new ProductClassDto {Id = x.Id, Name = x.Name, IsMedicamentClass = x.IsMedicamentClass}).ToListAsync(cancellationToken: cancellationToken);
            return query;

        }

        public async Task<IEnumerable<CatalogDto>> Handle(GetCatalogsForCustomerQuery request, CancellationToken cancellationToken)
        {
            var query = await _productClassRepository
                .Table
                .Where(x=>!x.IsMedicamentClass)
                .Select(x => new CatalogDto { Id = x.Id, Name = x.Name }).ToListAsync(cancellationToken: cancellationToken);
            return query;
        }
    }
}
