using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Manufacturers.DTOs;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Application.Catalog.Manufacturers.Queries
{
    public class GetManufacturersPagedQuery : ICommand<SyncPagedResult<ManufacturerDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
     public class  GetManufacturersPagedQueryHandler : ICommandHandler<GetManufacturersPagedQuery,SyncPagedResult<ManufacturerDto> >
     {
         private readonly IRepository<Manufacturer, Guid> _manufactureRepository;
         private readonly IMapper _mapper;

         public GetManufacturersPagedQueryHandler(IRepository<Manufacturer, Guid> manufactureRepository, IMapper mapper)
         {
             _manufactureRepository = manufactureRepository;
             _mapper = mapper;
         }
         public async Task<SyncPagedResult<ManufacturerDto>> Handle(GetManufacturersPagedQuery request, CancellationToken cancellationToken)
         {
             var query = _manufactureRepository.Table .AsNoTracking()
                 .Include(x=>x.Addresses)
                 .Include(x => x.PhoneNumbers)
                 .Include(x => x.Emails)
                 .DynamicWhereQuery(request.GridQuery);
             var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
             var result = await query
                 .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1,
                     request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
             var data = _mapper.Map<List<ManufacturerDto>>(result);
             return new SyncPagedResult<ManufacturerDto>{Result = data, Count = total};
         }
     }
}