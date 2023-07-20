using System;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.RequestLogs.Queries
{
    public class GetLogRequestQuery: ICommand<SyncPagedResult<LogRequest>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
    public class GetLogRequestQueryHandler :ICommandHandler<GetLogRequestQuery, SyncPagedResult<LogRequest>>
    {
        private readonly IRepository<LogRequest, Guid> _repository;

        public GetLogRequestQueryHandler(IRepository<LogRequest, Guid> repository)
        {
            _repository = repository;
        }
        public async Task<SyncPagedResult<LogRequest>> Handle(GetLogRequestQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.Table
                .DynamicWhereQuery(request.SyncDataGridQuery);
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var data =await query
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1, request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken: cancellationToken);

            return new SyncPagedResult<LogRequest> { Count = total, Result = data };
        }
    }
}