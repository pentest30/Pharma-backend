using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs.FinancialTransactions;
using GHPCommerce.Modules.Sales.Entities.Billing;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Queries.FinancialTransactions
{
    public class GetPagedFinancialTransactionQuery  : ICommand<SyncPagedResult<FinancialTransactionDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }

    }
      public class GetPagedFinancialTransactionQueryHandler : ICommandHandler<GetPagedFinancialTransactionQuery, SyncPagedResult<FinancialTransactionDto>>
    {
        private readonly IRepository<FinancialTransaction, Guid> _transRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetPagedFinancialTransactionQueryHandler(IRepository<FinancialTransaction, Guid> transRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper, ICommandBus commandBus,
            ICurrentUser currentUser)
        {
            _transRepository = transRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }

        public async Task<SyncPagedResult<FinancialTransactionDto>> Handle(GetPagedFinancialTransactionQuery request,
            CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _transRepository.Table
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(x => x.OrganizationId == orgId)
                .DynamicWhereQuery(request.SyncDataGridQuery);

            var result = await query
                .OrderByDescending(x => x.CreatedDateTime)
                .Page(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<FinancialTransactionDto>>(result);
            foreach (var invoice in data)
            {
                var createUser =
                    await _commandBus.SendAsync(new GetUserQuery {Id = invoice.CreatedByUserId}, cancellationToken);
                if (createUser != null) invoice.CreatedBy = createUser.UserName;
                if (invoice.UpdatedByUserId != default)
                {
                    var updateUser = await _commandBus.SendAsync(new GetUserQuery {Id = invoice.UpdatedByUserId},
                        cancellationToken);
                    if (updateUser != null) invoice.UpdatedBy = updateUser.UserName;
                }
            }


            return new SyncPagedResult<FinancialTransactionDto>
            {
                Result = data,
                Count = await query.CountAsync(cancellationToken)
            };
        }
    }
}