using AutoMapper;
using GHPCommerce.Application.Catalog.Dosages.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
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

namespace GHPCommerce.Application.Catalog.Dosages.Queries
{
    public class GetDosagesPagedQuery : ICommand<SyncPagedResult<DosageDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetDosagesPagedQueryHandler : ICommandHandler<GetDosagesPagedQuery, SyncPagedResult<DosageDto>>
    {
        private readonly IRepository<Dosage, Guid> _dosageRepository;
        private readonly IMapper _mapper;

        public GetDosagesPagedQueryHandler(IRepository<Dosage, Guid> FormRepository, IMapper mapper)
        {
            _dosageRepository = FormRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<DosageDto>> Handle(GetDosagesPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _dosageRepository.Table.AsNoTracking()
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Page(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<DosageDto>>(result);
            return new SyncPagedResult<DosageDto> { Result = data, Count = total };
        }
    }
}
