using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.DTOs;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class GetAllCustomersQuotaQuery : ICommand< List<CustomerQuotaDto>>
    {
        
    }
    public class GetAllCustomersQuotaQueryHandler : ICommandHandler<GetAllCustomersQuotaQuery, List<CustomerQuotaDto>>
    {
        private readonly IRepository<QuotaInitState, Guid> _quotaInitRepo;
        private readonly IQuotaRepository _quotaRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;

        public GetAllCustomersQuotaQueryHandler(IRepository<QuotaInitState, Guid> quotaInitRepo,
            IQuotaRepository quotaRepository,
            ICurrentOrganization currentOrganization, 
            ICurrentUser currentUser, 
            ICommandBus commandBus)
        {
            _quotaInitRepo = quotaInitRepo;
            _quotaRepository = quotaRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
        }
        public async Task<List<CustomerQuotaDto>> Handle(GetAllCustomersQuotaQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var currentUser = await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },  cancellationToken);

            var query =  _quotaInitRepo.Table
                .Where(x=>x.OrganizationId ==  orgId.Value );
            if (currentUser.UserRoles.Any(x => x.Role.Name == "Buyer") || currentUser.UserRoles.Any(x => x.Role.Name == "BuyerGroup"))
            {}
            else 
            {
                query = query.Where(x => x.QuotaId == _currentUser.UserId);
            }

            var r = await query.ToListAsync(cancellationToken: cancellationToken);
            var result = new List<CustomerQuotaDto>();
            var productIds = await _commandBus.SendAsync(new GetListProductsByIds { Ids = r.Select(x => x.ProductId).Distinct().ToList() },
                    cancellationToken);
            foreach (var initState in r)
            {
                var item = new CustomerQuotaDto();
                item.CustomerCode = initState.CustomerCode;
                item.CustomerName = initState.CustomerName;
                item.ProductCode = productIds.FirstOrDefault(f=> f.Id == initState.ProductId)?.Code;
                item.ProductName = productIds.FirstOrDefault(f=> f.Id == initState.ProductId)?.FullName;
                item.InitQuantity = initState.Quantity;
                item.QuotaDate = initState.DistributionDate.ToShortDateString();
                result.Add(item);
            }
           
            return result;
        }
    }
}