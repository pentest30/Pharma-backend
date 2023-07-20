using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using GHPCommerce.Domain.Repositories;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class DiscountsQueriesHandler :
        ICommandHandler<GetDiscountsListQuery, PagingResult<DiscountDto>>,
        ICommandHandler<GetActiveDiscountByProductQuery, IEnumerable<DiscountDto>>,
        ICommandHandler<GetPagedDiscountsQuery, SyncPagedResult<DiscountDto>>
    {
        private readonly IRepository<Discount, Guid> _discountsRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;

        public DiscountsQueriesHandler(IMapper mapper, IRepository<Discount, Guid> brandRepository, ICurrentOrganization currentOrganization
)
        {
            _mapper = mapper;
            _discountsRepository = brandRepository;
            _currentOrganization = currentOrganization;

        }
        public async Task<PagingResult<DiscountDto>> Handle(GetDiscountsListQuery request, CancellationToken cancellationToken)
        {
            var total = await _discountsRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _discountsRepository
                .Table
               
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<IEnumerable<DiscountDto>>(query);
            return new PagingResult<DiscountDto> { Data = data, Total = total };
        }

        public async Task<IEnumerable<DiscountDto>> Handle(GetActiveDiscountByProductQuery request, CancellationToken cancellationToken)
        {
           
            var orgId =request.OrganizationId ?? await _currentOrganization.GetCurrentOrganizationIdAsync();

            var query = await _discountsRepository
              .Table
              .Where(c => c.ProductId == request.ProductId 
                          && c.OrganizationId == orgId 
                          && DateTime.Now.Date >= c.from && DateTime.Now.Date < c.to && c.DiscountRate>0)
              .OrderByDescending(x => x.ThresholdQuantity)
              .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<DiscountDto>>(query);
        }

        public async Task<SyncPagedResult<DiscountDto>> Handle(GetPagedDiscountsQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query =  _discountsRepository.Table
                .Where(x=>x.OrganizationId == orgId)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
              .OrderByDescending(x => x.CreatedDateTime)
              .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1, request.SyncDataGridQuery.Take)
              .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<DiscountDto>>(result);
            return new SyncPagedResult<DiscountDto> { Result = data, Count = total};
        }
    }
}
