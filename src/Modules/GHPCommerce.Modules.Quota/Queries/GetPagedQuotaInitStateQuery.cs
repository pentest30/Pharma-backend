using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Quota.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class GetPagedQuotaInitStateQuery: ICommand<SyncPagedResult<QuotaInitState>>
    {
        public Guid SalesPersonId { get; set; }
        public DateTime DateTime { get; set; }
        public Guid ProductId { get; set; }
        public SyncDataGridQuery DataGridQuery { get; set; }
    }

    public class GetPagedQuotaInitStateQueryHandler : ICommandHandler<GetPagedQuotaInitStateQuery,SyncPagedResult<QuotaInitState>>
    {
        private readonly IRepository<QuotaInitState, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;

        public GetPagedQuotaInitStateQueryHandler(IRepository<QuotaInitState, Guid> repository, ICurrentOrganization currentOrganization)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
        }
        public async Task<SyncPagedResult<QuotaInitState>> Handle(GetPagedQuotaInitStateQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _repository.Table
                .AsNoTracking()
                .Where(x => x.OrganizationId == org
                            && x.QuotaId == request.SalesPersonId
                            && x.DistributionDate.Date == request.DateTime.Date
                            && x.ProductId == request.ProductId)
                .DynamicWhereQuery(request.DataGridQuery);
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query 
                //.OrderByDescending(x => x.QuotaDate)
                .Paged(request.DataGridQuery.Skip / request.DataGridQuery.Take +1, request.DataGridQuery.Take)
                .ToListAsync(cancellationToken: cancellationToken);
            return new SyncPagedResult<QuotaInitState>{ Count =total, Result = result };
        }
    }
}