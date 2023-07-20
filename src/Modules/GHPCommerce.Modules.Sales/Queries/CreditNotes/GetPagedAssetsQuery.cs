using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Customer.CreditNotes.DTOs;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using GHPCommerce.Modules.Sales.Entities.Billing;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Queries.CreditNotes
{
    public class GetPagedCreditNotesQuery : ICommand<SyncPagedResult<CreditNoteDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }

    }

    public class GetPagedCreditNotesQueryHandler : ICommandHandler<GetPagedCreditNotesQuery, SyncPagedResult<CreditNoteDto>>
    {
        private readonly IRepository<CreditNote, Guid> _creditNoteRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetPagedCreditNotesQueryHandler(IRepository<CreditNote, Guid> creditNoteRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper, ICommandBus commandBus,
            ICurrentUser currentUser)
        {
            _creditNoteRepository = creditNoteRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }

        public async Task<SyncPagedResult<CreditNoteDto>> Handle(GetPagedCreditNotesQuery request,
            CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var query = _creditNoteRepository.Table
                .Include(c => c.CreditNoteItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .Where(x => x.OrganizationId == orgId)
                .DynamicWhereQuery(request.SyncDataGridQuery);

            var result = await query
                .OrderByDescending(x => x.CreatedDateTime)
                .Page(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var creditNotes = _mapper.Map<List<CreditNoteDto>>(result);
            foreach (var creditNote in creditNotes)
            { 
                var createUser =
                    await _commandBus.SendAsync(new GetUserQuery { Id = creditNote.CreatedByUserId }, cancellationToken);
                if (createUser != null) creditNote.CreatedBy = createUser.UserName;
                if (creditNote.UpdatedByUserId != default)
                {
                    var updateUser = await _commandBus.SendAsync(new GetUserQuery { Id = creditNote.UpdatedByUserId },
                        cancellationToken);
                    if (updateUser != null) creditNote.UpdatedBy = updateUser.UserName;
                }
            }


            return new SyncPagedResult<CreditNoteDto>
            {
                Result = creditNotes,
                Count = await query.CountAsync(cancellationToken)
            };
        }
    }
}