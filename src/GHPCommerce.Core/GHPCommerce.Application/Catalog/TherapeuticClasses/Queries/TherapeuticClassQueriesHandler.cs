using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.TherapeuticClasses.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Queries
{
    public class TherapeuticClassQueriesHandler :
        ICommandHandler<GetTherapeuticClassesListQuery, PagingResult<TherapeuticClassDto>>,
        ICommandHandler<GetTherapeuticClassByIdQuery, TherapeuticClassDto>,
        ICommandHandler<GetAllTherapeuticClassesQuery, IEnumerable<TherapeuticClassDto>>
    {
        private readonly IRepository<TherapeuticClass, Guid> _therapeuticClassRepository;
        private readonly IMapper _mapper;

        public TherapeuticClassQueriesHandler(IRepository<TherapeuticClass, Guid> therapeuticClassRepository, IMapper mapper)
        {
            _therapeuticClassRepository = therapeuticClassRepository;
            _mapper = mapper;
        }
        public async Task<PagingResult<TherapeuticClassDto>> Handle(GetTherapeuticClassesListQuery request, CancellationToken cancellationToken)
        {
            var total = await _therapeuticClassRepository
                .Table.CountAsync(cancellationToken: cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _therapeuticClassRepository
                .Table
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);

            var data =  _mapper.Map<IEnumerable<TherapeuticClassDto>>(query);
            var therapeuticClassDtos = data as TherapeuticClassDto[] ?? data.ToArray();
            return new PagingResult<TherapeuticClassDto> { Data = therapeuticClassDtos, Total = total };
        }

        public async Task<TherapeuticClassDto> Handle(GetTherapeuticClassByIdQuery request, CancellationToken cancellationToken)
        {
            var therapeuticCLass = await _therapeuticClassRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (therapeuticCLass == null)
                throw new NotFoundException($"therapeutic class with id: {request.Id} was not found");

            return _mapper.Map<TherapeuticClassDto>(therapeuticCLass);
        }

        public async Task<IEnumerable<TherapeuticClassDto>> Handle(GetAllTherapeuticClassesQuery request, CancellationToken cancellationToken)
        {
            var query = await _therapeuticClassRepository
                .Table
               
                .ToListAsync(cancellationToken: cancellationToken);

            return _mapper.Map<IEnumerable<TherapeuticClassDto>>(query);
        }
    }
}
