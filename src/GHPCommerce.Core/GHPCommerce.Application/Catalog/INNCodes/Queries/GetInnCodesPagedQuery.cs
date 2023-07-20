using AutoMapper;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Application.Catalog.INNCodes.Queries
{
    public class GetInnCodesPagedQuery : ICommand<SyncPagedResult<InnCodeDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetInnCodesPagedQueryHandler : ICommandHandler<GetInnCodesPagedQuery, SyncPagedResult<InnCodeDto>>
    {
        private readonly IRepository<INNCode, Guid> _INNCodeRepository;
        private readonly IMapper _mapper;

        public GetInnCodesPagedQueryHandler(IRepository<INNCode, Guid> INNCodeRepository, IMapper mapper)
        {
            _INNCodeRepository = INNCodeRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<InnCodeDto>> Handle(GetInnCodesPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _INNCodeRepository.Table.Include(x=>x.Form)
                .DynamicWhereQuery(request.GridQuery);
            var total = await query.CountAsync( cancellationToken: cancellationToken);
            var result =  await query
                .Page(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take)
                .ToListAsync(cancellationToken: cancellationToken);
            var data = new List<InnCodeDto>();
           foreach (var item in result)
            {
                data.Add(new InnCodeDto(item.Id,item.Name, item.Form.Name, item.Form.Id));
            }
         
            return new SyncPagedResult<InnCodeDto> { Result = data, Count = total };
        }
    }
}
