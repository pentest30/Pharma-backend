using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Packaging.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace GHPCommerce.Application.Catalog.Packaging.Queries
{
    public class PackagingQueriesHandler : 
        ICommandHandler<GetPackagingListQuery, PagingResult<PackagingDto>>,
        ICommandHandler<GetPackagingByIdQuery, PackagingDto>,
        ICommandHandler<GetAllPackagingQuery, IEnumerable<PackagingDto>>
    {
        private readonly IRepository<Domain.Domain.Catalog.Packaging, Guid> _packagingRepository;
        private readonly IMapper _mapper;

        public PackagingQueriesHandler(IRepository<Domain.Domain.Catalog.Packaging, Guid> packagingRepository, IMapper mapper)
        {
            _packagingRepository = packagingRepository;
            _mapper = mapper;
        }
        public async Task<PagingResult<PackagingDto>> Handle(GetPackagingListQuery request, CancellationToken cancellationToken)
        {
            var total = await _packagingRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _packagingRepository
                .Table.OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data =  _mapper.Map<IEnumerable<PackagingDto>>(query);
            return new PagingResult<PackagingDto> { Data = data, Total = total };
        }

        public async Task<PackagingDto> Handle(GetPackagingByIdQuery request, CancellationToken cancellationToken)
        {
            var packaging = await _packagingRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (packaging == null)
                throw new NotFoundException($"packaging with id: {request.Id} was not found");
            return _mapper.Map<PackagingDto>(packaging);
        }

        public async Task<IEnumerable<PackagingDto>> Handle(GetAllPackagingQuery request, CancellationToken cancellationToken)
        {
            var query = await _packagingRepository
                .Table
                .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<PackagingDto>>(query);
        }
    }
}
