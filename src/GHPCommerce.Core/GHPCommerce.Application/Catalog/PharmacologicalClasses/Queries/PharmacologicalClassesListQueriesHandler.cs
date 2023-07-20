using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Queries
{
    public class PharmacologicalClassesListQueriesHandler :
        ICommandHandler<GetPharmacologicalClassesListQuery, PagingResult<PharmacologicalClassDto>>,
        ICommandHandler<GetPharmacologicalClassByIdQuery, PharmacologicalClassDto>,
        ICommandHandler<GetAllPharmacologicalClassesQuery, IEnumerable<PharmacologicalClassDto>>
    {
        private readonly IRepository<PharmacologicalClass, Guid> _pharmacologicalClassesRepository;
        private readonly IMapper _mapper;

        public PharmacologicalClassesListQueriesHandler(IRepository<PharmacologicalClass, Guid> pharmacologicalClassesRepository, IMapper mapper)
        {
            _pharmacologicalClassesRepository = pharmacologicalClassesRepository;
            _mapper = mapper;
        }
        public async Task<PagingResult<PharmacologicalClassDto>> Handle(GetPharmacologicalClassesListQuery request, CancellationToken cancellationToken)
        {
            var total = await _pharmacologicalClassesRepository
             .Table.CountAsync(cancellationToken: cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _pharmacologicalClassesRepository
                .Table
                .Paged(request.Page, request.PageSize)
                .OrderBy(orderQuery)
                .ToListAsync(cancellationToken: cancellationToken);
            var data=  _mapper.Map<IEnumerable<PharmacologicalClassDto>>(query);
            return new PagingResult<PharmacologicalClassDto> { Data = data, Total = total };
        }

        public async Task<PharmacologicalClassDto> Handle(GetPharmacologicalClassByIdQuery request, CancellationToken cancellationToken)
        {
            var pharmacologicalClass = await _pharmacologicalClassesRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (pharmacologicalClass == null)
                throw new NotFoundException($"pharmacological Class with id: {request.Id} was not found");

            return _mapper.Map<PharmacologicalClassDto>(pharmacologicalClass);
        }

        public async Task<IEnumerable<PharmacologicalClassDto>> Handle(GetAllPharmacologicalClassesQuery request, CancellationToken cancellationToken)
        {
            var query = await _pharmacologicalClassesRepository
                .Table
               
                .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<PharmacologicalClassDto>>(query);
        }
    }
}
