using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Quota.Queries.ReceivedQuota
{
    public class GetTotalRequestQuotaQuery : ICommand<int>
    {
        
    }
    public  class GetTotalRequestQuotaQueryHandler :ICommandHandler<GetTotalRequestQuotaQuery, int>
    {
        private readonly IQuotaRequestRepository _requestRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;
        private readonly ICommandBus _commandBus;

        public GetTotalRequestQuotaQueryHandler( IQuotaRequestRepository requestRepository,
            ICurrentOrganization currentOrganization, 
            ICurrentUser currentUser, ICommandBus commandBus)
        {
            _requestRepository = requestRepository;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _commandBus = commandBus;
        }
       
        public async Task<int> Handle(GetTotalRequestQuotaQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return 0;
            var query = _requestRepository.Table.Where(x => x.OrganizationId == org&& x.Status == QuotaRequestStatus.Wait);
            var currentUser = await _commandBus.SendAsync(new GetUserQuery {Id = _currentUser.UserId, IncludeRoles = true},cancellationToken);
            if (currentUser.UserRoles.Any(x => x.Role.Name != "Buyer"))
                query = query.Where(x => x.DestSalesPersonId == currentUser.Id );
            else query =  query.Where(x => x.ForBuyer == true);

            var total = await query.CountAsync( cancellationToken: cancellationToken);
            return total;
        }
    }
}