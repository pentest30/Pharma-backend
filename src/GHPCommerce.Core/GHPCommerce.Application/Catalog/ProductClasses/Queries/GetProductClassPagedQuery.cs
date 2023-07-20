using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.ProductClasses.DTOs;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Application.Catalog.ProductClasses.Queries
{
    public class GetProductClassPagedQuery : ICommand<SyncPagedResult<ProductClassDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
    public class GetProductClassPagedQueryHandler : ICommandHandler<GetProductClassPagedQuery, SyncPagedResult<ProductClassDto>>
    {
        private readonly IRepository<ProductClass, Guid> _productClassRepository;
        private readonly IMapper _mapper;

        public GetProductClassPagedQueryHandler(IRepository<ProductClass, Guid> productClassRepository, IMapper mapper)
        {
            _productClassRepository = productClassRepository;
            _mapper = mapper;
        }
        public  async Task<SyncPagedResult<ProductClassDto>> Handle(GetProductClassPagedQuery request, CancellationToken cancellationToken)
        {
            var query = _productClassRepository.Table
                .DynamicWhereQuery(request.GridQuery);
            var total = await EntityFrameworkDynamicQueryableExtensions.CountAsync(query, cancellationToken: cancellationToken);
            var result = await query
                .Paged(request.GridQuery.Skip / request.GridQuery.Take + 1,
                    request.GridQuery.Take).ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<ProductClassDto>>(result);
            return new SyncPagedResult<ProductClassDto>
                {Result = data, Count = total};
        }
    }
}