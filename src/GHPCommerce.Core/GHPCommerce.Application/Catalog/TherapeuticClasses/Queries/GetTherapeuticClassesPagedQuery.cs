using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.TherapeuticClasses.DTOs;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Queries
{
    public class GetTherapeuticClassesPagedQuery : ICommand<SyncPagedResult<TherapeuticClassDto>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }
    }
     public class GetTherapeuticClassesPagedQueryHandler : ICommandHandler<GetTherapeuticClassesPagedQuery, SyncPagedResult<TherapeuticClassDto>>
     {
         private readonly IRepository<TherapeuticClass, Guid> _therapeuticClassRepository;
         private readonly IMapper _mapper;

         public GetTherapeuticClassesPagedQueryHandler(IRepository<TherapeuticClass, Guid> therapeuticClassRepository, IMapper mapper)
         {
             _therapeuticClassRepository = therapeuticClassRepository;
             _mapper = mapper;
         }
         public async Task<SyncPagedResult<TherapeuticClassDto>> Handle(GetTherapeuticClassesPagedQuery request, CancellationToken cancellationToken)
         {
             var query = _therapeuticClassRepository.Table
                 .DynamicWhereQuery(request.DataGridQuery);
             var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
             var result = await query
                 .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1,
                     request.DataGridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
             var data = _mapper.Map<List<TherapeuticClassDto>>(result);
             return new SyncPagedResult<TherapeuticClassDto>
                 {Result = data, Count = total};
         }
     }
}