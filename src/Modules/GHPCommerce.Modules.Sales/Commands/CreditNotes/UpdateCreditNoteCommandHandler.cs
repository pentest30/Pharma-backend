using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Batches.Queries;

using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Queries;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Commands.CreditNotes
{

    public  class  UpdateCreditNoteCommandHandler : ICommandHandler<UpdateCreditNoteCommand, ValidationResult>
    {
          
        private readonly IRepository<Invoice, Guid> _invoicesRepository; 
        private readonly IRepository<CreditNote, Guid> _creditNotesRepository;
        private readonly IRepository<FinancialTransaction, Guid> _transactionRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ISequenceNumberService<CreditNote, Guid> _sequenceNumberService;
        private  SalesDbContext _context;

        public UpdateCreditNoteCommandHandler(
             
            IRepository<FinancialTransaction, Guid> transactionRepository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            ICurrentUser currentUser,
            ICommandBus commandBus,
            SalesDbContext context,
            ISequenceNumberService<CreditNote, Guid> sequenceNumberService, IRepository<Invoice, Guid> invoicesRepository, 
            IRepository<CreditNote, Guid> creditNotesRepository)
        {
             
            _currentOrganization = currentOrganization; 
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _sequenceNumberService = sequenceNumberService;
            _context = context;
            _invoicesRepository = invoicesRepository;
            _creditNotesRepository = creditNotesRepository;
        }
        private async Task<ValidationResult> CreditNoteCanBeUpdated(UpdateCreditNoteCommand request, CancellationToken cancellationToken)
        {
            bool response = true;
            //Existing Invoice

            var creditNote = await _creditNotesRepository.Table.Where(c => c.Id == request.Id).Include(c => c.CreditNoteItems).FirstOrDefaultAsync(cancellationToken);
            if (creditNote == null) return new ValidationResult() { Errors = { new ValidationFailure("Erreur", "Avoir introuvable !")} };
            if(creditNote.State!=Core.Shared.Enums.CreditNoteState.Draft) return new ValidationResult() { Errors = { new ValidationFailure("Erreur", "Avoir non modifiable !") } };
            if (request.ClaimDate.HasValue && (creditNote.InvoiceDate.Date > request.ClaimDate.Value.ToLocalTime().Date || request.ClaimDate.Value.ToLocalTime().Date > DateTime.Today)) return new ValidationResult() { Errors = { new ValidationFailure("Erreur", "Date réclamation incorrecte !") } };
            if (await _creditNotesRepository.Table.AnyAsync(c =>
            c.Id!=request.Id &&
            c.ClaimNumber == request.ClaimNumber && c.ClaimDate.Value.Year == 
            request.ClaimDate.Value.Year, cancellationToken))
                return new ValidationResult() { Errors = { new ValidationFailure("Erreur", "Un avoir a été déjà édité avec la même réclamation !") } };
            //Get product/batches from invoices & calculated already returned quantities
            var creditNotes = await _creditNotesRepository.Table.Include(c => c.CreditNoteItems).Where(c => c.InvoiceId == creditNote.InvoiceId && c.Id!= creditNote.Id).ToListAsync(cancellationToken);
            var invoice = await _invoicesRepository.Table.Where(c => c.Id == creditNote.InvoiceId).Include(c => c.InvoiceItems)
                .FirstOrDefaultAsync(cancellationToken);
            var lines = invoice.InvoiceItems.Select(l => new {
                productId = l.ProductId,
                internalBatchNumber = l.InternalBatchNumber,
                quantity = l.Quantity
            });
            //Cannot return not invoiced line
            request.CreditNoteItems.ForEach(i => {
                var line = lines.FirstOrDefault(l => l.productId == i.ProductId && l.internalBatchNumber == i.InternalBatchNumber);
                if (line == null)
                {
                    response = false;
                    return;
                }
                int returnedQty = 0;
                if (creditNotes != null)
                {

                    creditNotes.ForEach(
                        a =>
                        {
                            var returnedLine = a.CreditNoteItems.FirstOrDefault(l => l.ProductId == line.productId && l.InternalBatchNumber == line.internalBatchNumber);
                            if (returnedLine != null)
                            {
                                returnedQty += returnedLine.Quantity;
                            }
                        }
                        );
                }
                if (line.quantity - returnedQty < i.Quantity)
                {
                    response = false;
                    return;
                }

            });
            if (!response)  return new ValidationResult() { Errors = { new ValidationFailure("Erreur", "La quantité retournée ne doit pas dépasser la quantité facturée !") } };
            //Cannot return a quantity not invoiced


            return default; 
        }
        public async Task<ValidationResult> Handle(UpdateCreditNoteCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException(""); 
            try
            { 
                var result = await CreditNoteCanBeUpdated(request, cancellationToken);
                if (result != default)
                { 
                    return result;
                }
                var creditNote = await _creditNotesRepository.Table.Where(c => c.Id == request.Id).Include(c => c.CreditNoteItems).FirstOrDefaultAsync(cancellationToken);
                if (creditNote == null) return new ValidationResult() { Errors = { new ValidationFailure("Erreur", "Avoir introuvable !") } };
                var invoice = await _invoicesRepository.Table.Include(c => c.InvoiceItems)
                    .OrderByDescending(x => x.UpdatedDateTime)
                    .FirstOrDefaultAsync(x => creditNote.InvoiceId == x.Id, cancellationToken: cancellationToken);

                var customer = await _commandBus.SendAsync(
                    new GetCustomerByOrganizationIdQuery { OrganizationId = (Guid)invoice.OrganizationId }, cancellationToken);
                var currentUser =
                    await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                        cancellationToken);

                
                creditNote.CreditNoteItems = _mapper.Map<List<CreditNoteItem>>(request.CreditNoteItems);
                var i = 0;

                creditNote.ClaimDate = request.ClaimDate;
                creditNote.ClaimNote = request.ClaimNote;
                creditNote.ClaimNumber = request.ClaimNumber;
                creditNote.ClaimReason = request.ClaimReason;
                
                foreach (var item in creditNote.CreditNoteItems)
                {
                    var invoicedItem = invoice.InvoiceItems.Find(c =>
                        c.ProductId == item.ProductId && c.InternalBatchNumber == item.InternalBatchNumber);
                    var batch = await _commandBus.SendAsync(new GetBatchByIdQuery
                    {
                        ProductId = item.ProductId,
                        InternalBatchNumber = item.InternalBatchNumber, 
                        VendorBatchNumber = item.VendorBatchNumber
                    }, cancellationToken);
                    if (batch != null)
                    {
                        item.PurchaseDiscountUnitPrice = (decimal)(batch.PurchaseUnitPrice -
                                                                    (batch.PurchaseUnitPrice *
                                                                     batch.PurchaseDiscountRatio));
                    
                    }
                    else
                    {
                        item.PurchaseDiscountUnitPrice = 0; 
                    }
                        item.LineNum = i + 1;
                    item.PpaTTC = item.PpaHT + ((decimal)item.Tax * item.PpaHT);
                    item.TotalExlTax = item.Quantity * item.UnitPrice;
                    item.TotalDiscount =((decimal)(invoicedItem.DiscountRate)) *item.TotalExlTax ;
                    item.TotalTax = item.Quantity * ((decimal)item.Tax * item.UnitPrice);
                    
                    item.TotalInlTax = ((item.Quantity * item.UnitPrice) - item.TotalDiscount) + item.TotalTax;
                    item.DiscountRate = invoicedItem.DiscountRate;
                    item.DisplayedTotalDiscount = (decimal)item.DisplayedDiscountRate * item.TotalExlTax;
                  
                }

                creditNote.TotalTax = creditNote.CreditNoteItems.Sum(c => c.TotalTax);
                creditNote.TotalDiscount = creditNote.CreditNoteItems.Sum(c => c.TotalDiscount);
                creditNote.TotalHT = creditNote.CreditNoteItems.Sum(c => c.TotalExlTax);
                creditNote.TotalTTC = creditNote.CreditNoteItems.Sum(c => c.TotalInlTax);
                creditNote.TotalDisplayedDiscount = creditNote.CreditNoteItems.Sum(c => c.DisplayedTotalDiscount);
                 


                var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(creditNote.CreditNoteDate, orgId.Value);
                creditNote.SequenceNumber = sq;

                _creditNotesRepository.Update(creditNote);
                
                await _creditNotesRepository.UnitOfWork.SaveChangesAsync(); 
            }
            catch (Exception e)
            {
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", e.Message) }};
                return validations;
            }           
            return default;
        }
    }
}