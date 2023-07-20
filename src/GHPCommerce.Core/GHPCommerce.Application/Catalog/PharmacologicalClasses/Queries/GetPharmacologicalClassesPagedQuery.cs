using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.DTOs;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Queries
{
    public class GetPharmacologicalClassesPagedQuery : ICommand<SyncPagedResult<PharmacologicalClassDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetPharmacologicalClassesPagedQueryHandler : ICommandHandler<GetPharmacologicalClassesPagedQuery, SyncPagedResult<PharmacologicalClassDto>>
    {
        private readonly IRepository<PharmacologicalClass, Guid> _pharmacologicalClassesRepository;
        private readonly IMapper _mapper;

        public GetPharmacologicalClassesPagedQueryHandler(IRepository<PharmacologicalClass, Guid> pharmacologicalClassesRepository, IMapper mapper)
        {
            _pharmacologicalClassesRepository = pharmacologicalClassesRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<PharmacologicalClassDto>> Handle(GetPharmacologicalClassesPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _pharmacologicalClassesRepository.Table
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<PharmacologicalClassDto>>(result);
            return new SyncPagedResult<PharmacologicalClassDto>{Result = data, Count = total};
        }
    }
}