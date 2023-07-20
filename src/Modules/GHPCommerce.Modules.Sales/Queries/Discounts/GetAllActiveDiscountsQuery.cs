using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs.Discounts;
using GHPCommerce.Modules.Sales.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace GHPCommerce.Modules.Sales.Queries.Discounts
{
    public class GetAllActiveDiscountsQuery : ICommand<List<DiscountDtoV1>>
    {
        public Guid OrganizationId { get; set; }
    }

    public class GetAllActiveDiscountsQueryHandler : ICommandHandler<GetAllActiveDiscountsQuery, List<DiscountDtoV1>>
    {
        private readonly IRepository<Discount, Guid> _discountsRepository;
        private readonly IMapper _mapper;
        private readonly ICurrentOrganization _currentOrganization;

        public GetAllActiveDiscountsQueryHandler(IMapper mapper, IRepository<Discount, Guid> brandRepository, ICurrentOrganization currentOrganization
        )
        {
            _mapper = mapper;
            _discountsRepository = brandRepository;
            _currentOrganization = currentOrganization;

        }
        public async Task<List<DiscountDtoV1>> Handle(GetAllActiveDiscountsQuery request, CancellationToken cancellationToken)
        {
            var query = await _discountsRepository
                .Table
                .Where(c => c.OrganizationId == request.OrganizationId && DateTime.Now.Date >= c.from && DateTime.Now.Date < c.to && c.DiscountRate>0)
                .ToListAsync(cancellationToken: cancellationToken);
            var data = _mapper.Map<List<DiscountDtoV1>>(query);
            return data;
        }
    }

}