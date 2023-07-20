using AutoMapper;
using GHPCommerce.Application.Catalog.Forms.DTOs;
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

namespace GHPCommerce.Application.Catalog.Forms.Queries
{
    public class GetFormsPagedQuery : ICommand<SyncPagedResult<FormDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetFormsPagedQueryHandler : ICommandHandler<GetFormsPagedQuery, SyncPagedResult<FormDto>>
    {
        private readonly IRepository<Form, Guid> _formRepository;
        private readonly IMapper _mapper;

        public GetFormsPagedQueryHandler(IRepository<Form, Guid> FormRepository, IMapper mapper)
        {
            _formRepository = FormRepository;
            _mapper = mapper;
        }
        public async Task<SyncPagedResult<FormDto>> Handle(GetFormsPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _formRepository.Table.AsNoTracking()
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Page(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<FormDto>>(result);
            return new SyncPagedResult<FormDto> { Result = data, Count = total };
        }
    }
}
