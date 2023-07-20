using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs.Invoices;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public class GetInvoiceByIdQuery : ICommand<InvoiceDto>
    {
        public Guid Id { get; set; }
    }
    public class GetInvoiceByIdQueryHandler : ICommandHandler<GetInvoiceByIdQuery, InvoiceDto>
    {
        private readonly IRepository<Invoice, Guid> _invoicerepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IRepository<CreditNote, Guid> _creditNotesRepository;
        private readonly IMapper _mapper;

        public GetInvoiceByIdQueryHandler(IRepository<Invoice, Guid> repository,
            ICurrentOrganization currentOrganization,
            IMapper mapper, IRepository<CreditNote, Guid> creditNotesRepository)
        {
            _invoicerepository = repository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _creditNotesRepository = creditNotesRepository;
        }
        public async Task<InvoiceDto> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            var invoice = await _invoicerepository.Table.Include(c => c.InvoiceItems)
                .OrderByDescending(x => x.CreatedDateTime)
                .FirstOrDefaultAsync(x => x.OrganizationId == orgId && request.Id == x.Id, cancellationToken: cancellationToken);
            if (invoice == null) return null;
            var creditNotes = await _creditNotesRepository.Table.Include(c => c.CreditNoteItems).Where(c => c.InvoiceId == invoice.Id).ToListAsync(cancellationToken);
            var lines = invoice.InvoiceItems.Select(l => new
            {
                productId = l.ProductId,
                internalBatchNumber = l.InternalBatchNumber,
                quantity = l.Quantity
            });
            //Cannot return not invoiced line
            var invoiceDto = _mapper.Map<InvoiceDto>(invoice);
            invoiceDto.InvoiceItems.ForEach(i =>
            {

                int returnedQty = 0;
                if (creditNotes != null)
                {

                    creditNotes.ForEach(
                        a =>
                        {
                            var returnedLine = a.CreditNoteItems.FirstOrDefault(l => l.ProductId == i.ProductId &&
                            l.InternalBatchNumber == i.InternalBatchNumber);
                            if (returnedLine != null)
                            {
                                returnedQty += returnedLine.Quantity;
                            }
                        }
                        );
                }
                i.ReturnedQty = returnedQty;
            });

            return invoiceDto;

        }

    }
}