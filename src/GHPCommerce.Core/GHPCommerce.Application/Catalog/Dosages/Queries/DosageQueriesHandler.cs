using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Dosages.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace GHPCommerce.Application.Catalog.Dosages.Queries
{
    public class DosageQueriesHandler :
        ICommandHandler<GetDosagesListQuery, PagingResult<DosageDto>>,
        ICommandHandler<GetDosageByIdQuery, DosageDto>,
        ICommandHandler<GetAllDosagesQuery, IEnumerable<DosageDto>>

    {
        private readonly IRepository<Dosage, Guid> _dosageRepository;
        private readonly IMapper _mapper;
        public DosageQueriesHandler(IRepository<Dosage, Guid> dosageRepository, IMapper mapper)
        {
            _dosageRepository = dosageRepository;
            _mapper = mapper;
        }
        public async  Task<PagingResult<DosageDto>> Handle(GetDosagesListQuery request, CancellationToken cancellationToken)
        {
            var total = await _dosageRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _dosageRepository
                .Table.OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data =  _mapper.Map<IEnumerable<DosageDto>>(query);
            return new PagingResult<DosageDto> { Data = data, Total = total };

        }

        public async Task<DosageDto> Handle(GetDosageByIdQuery request, CancellationToken cancellationToken)
        {
            var dosage = await _dosageRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);

            if (dosage == null)
                throw new NotFoundException($"dosage with id: {request.Id} was not found");
            return _mapper.Map<DosageDto>(dosage);
        }

        public async Task<IEnumerable<DosageDto>> Handle(GetAllDosagesQuery request, CancellationToken cancellationToken)
        {
            var query = await _dosageRepository.Table
                .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<DosageDto>>(query);
        }

    }
}
