using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.DTOs;
using GHPCommerce.Modules.Quota.Repositories;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Quota.Queries.QuotaRequest
{
    public class QuotaRequestQueriesHandler : ICommandHandler<GetPagedRequestedQuotasQuery, SyncPagedResult<QuotaRequestDto>>
    {
      
        private readonly IQuotaRequestRepository _requestRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public QuotaRequestQueriesHandler(
            IQuotaRequestRepository requestRepository,
            ICurrentOrganization currentOrganization, 
            ICurrentUser currentUser ,
            ICommandBus commandBus, 
            IMapper mapper)
        {
          
            _requestRepository = requestRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
            _mapper = mapper;
        }

        public async Task<SyncPagedResult<QuotaRequestDto>> Handle(GetPagedRequestedQuotasQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<QuotaRequestDto>();
            var currentUser =
                await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                    cancellationToken);
            var predicateBuilder = PredicateBuilder.New<Entities.QuotaRequest>();
            var query = _requestRepository.Table.Where(x => x.OrganizationId == org);
            if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson"))
                query = query.Where(x => x.SalesPersonId == currentUser.Id).DynamicWhereQuery(request.DataGridQuery);
            else if (currentUser.UserRoles.Any(x => x.Role.Name == "Buyer") || currentUser.UserRoles.Any(x => x.Role.Name == "BuyerGroup"))
                query = query.Where(x => x.ForBuyer).DynamicWhereQuery(request.DataGridQuery);
            else if (currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor"))
            {
                var ids = await _commandBus.SendAsync(new GetSalesPersonsBySupervisorQuery { Id = currentUser.Id },
                    cancellationToken);
                var enumerable = ids as Guid[] ?? ids.ToArray();
                foreach (var id in enumerable)
                {
                    predicateBuilder.Or(x => x.SalesPersonId == id );
                }
                predicateBuilder.Or(x => x.SalesPersonId == _currentUser.UserId);
                query = query.Where(predicateBuilder).DynamicWhereQuery(request.DataGridQuery);
                
            }
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            query = query
                .OrderByDescending(x => x.Date)
                .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take + 1, request.DataGridQuery.Take);
            var quotaList = _mapper.Map<List<QuotaRequestDto>>(await query.ToListAsync(cancellationToken));
            return new SyncPagedResult<QuotaRequestDto> { Count = total, Result = quotaList };

        }
    }
}