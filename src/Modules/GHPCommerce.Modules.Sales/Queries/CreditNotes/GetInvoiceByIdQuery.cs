using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Customer.CreditNotes.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs.CreditNotes;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Queries.CreditNotes
{
    public class GetCreditNoteByIdQuery : ICommand<CreditNoteDto>
    {
        public Guid Id { get; set; }
    }
    public class GetCreditNoteByIdQueryHandler : ICommandHandler<GetCreditNoteByIdQuery, CreditNoteDto>
    { 
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IRepository<CreditNote, Guid> _creditNotesRepository;
        private readonly IMapper _mapper;

        public GetCreditNoteByIdQueryHandler( 
            ICurrentOrganization currentOrganization,
            IMapper mapper, IRepository<CreditNote, Guid> creditNotesRepository)
        { 
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _creditNotesRepository = creditNotesRepository;
        }
        public async Task<CreditNoteDto> Handle(GetCreditNoteByIdQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var creditNote = await _creditNotesRepository.Table.Include(c => c.CreditNoteItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .FirstOrDefaultAsync(x => x.OrganizationId == orgId && request.Id == x.Id, cancellationToken: cancellationToken);

            var creditNoteDto = _mapper.Map<CreditNoteDto>(creditNote);
            return creditNoteDto;

        }

    }
}