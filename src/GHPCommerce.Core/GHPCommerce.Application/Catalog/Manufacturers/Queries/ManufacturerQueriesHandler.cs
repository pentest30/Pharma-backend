using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Brands.Queries;
using GHPCommerce.Application.Catalog.Manufacturers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.Manufacturers.Queries
{
    public class ManufacturerQueriesHandler :
        ICommandHandler<GetManufacturersListQuery, PagingResult<ManufacturerDto>>,
        ICommandHandler<GetManufacturerByIdQuery, ManufacturerDto>,
        ICommandHandler<GetAllManufacturersQuery, IEnumerable<ManufacturerDto>>,
        ICommandHandler<GetBrandsByCatalogIdQuery, IEnumerable<BrandDtoV1>>
    {
        private readonly IRepository<Manufacturer, Guid> _manufactureRepository;
        private readonly IMapper _mapper;

        public ManufacturerQueriesHandler(IRepository<Manufacturer, Guid> manufactureRepository, IMapper mapper)
        {
            _manufactureRepository = manufactureRepository;
            _mapper = mapper;
        }
        public async Task<PagingResult<ManufacturerDto>> Handle(GetManufacturersListQuery request, CancellationToken cancellationToken)
        {
            var total = await _manufactureRepository
              .Table.CountAsync(cancellationToken: cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _manufactureRepository
                .Table
                .AsNoTracking()
                .Include(x=>x.Addresses)
                .Include(x => x.PhoneNumbers)
                .Include(x => x.Emails)
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<IEnumerable<ManufacturerDto>>(query);
          
            return new PagingResult<ManufacturerDto> { Data = data, Total = total };
            
        }
        public async Task<ManufacturerDto> Handle(GetManufacturerByIdQuery request, CancellationToken cancellationToken)
        {
            var manufacturer = await _manufactureRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (manufacturer == null)
                throw new NotFoundException($"manufacturer with id: {request.Id} was not found");
            return _mapper.Map<ManufacturerDto>(manufacturer);
        }

        public  async Task<IEnumerable<ManufacturerDto>> Handle(GetAllManufacturersQuery request, CancellationToken cancellationToken)
        {
            var query = await _manufactureRepository
                .Table.Select(x=> new ManufacturerDto { Name = x.Name, Id = x.Id})
                .ToListAsync(cancellationToken: cancellationToken);
            return query;
        }

        public async Task<IEnumerable<BrandDtoV1>> Handle(GetBrandsByCatalogIdQuery request, CancellationToken cancellationToken)
        {
            var query = await _manufactureRepository.Table
                .Include(x => x.Products)
                .Include("Products.Manufacturer")
                .SelectMany(x => x.Products)
                .Where(x => x.ProductClassId == request.CatalogId &&! string.IsNullOrEmpty(x.Manufacturer.Name))
                .Select(x => new BrandDtoV1 {Id = x.ManufacturerId.Value, Name = x.Manufacturer.Name, Qnt = x.Manufacturer.Products.Count})
                .Distinct()
                .OrderByDescending(x=>x.Qnt)
                .Take(20)
                .ToListAsync(cancellationToken: cancellationToken);
            return query;
        }
    }
}
