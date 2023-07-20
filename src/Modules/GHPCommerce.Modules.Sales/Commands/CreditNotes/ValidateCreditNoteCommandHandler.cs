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
using MassTransit;
using GHPCommerce.Core.Shared.Events.CreditNotes;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;

namespace GHPCommerce.Modules.Sales.Commands.CreditNotes
{

    public  class  ValidateCreditNoteCommandHandler : ICommandHandler<ValidateCreditNoteCommand, ValidationResult>
    {
        
        private readonly IRepository<CreditNote, Guid> _creditNotesRepository;
        private readonly IRepository<FinancialTransaction, Guid> _transactionRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ISequenceNumberService<CreditNote, Guid> _sequenceNumberService;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly IPublishEndpoint _bus;
        private  SalesDbContext _context;
        private readonly IMapper _mapper;

        public ValidateCreditNoteCommandHandler(
            IRepository<CreditNote, Guid> repository,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            ICommandBus commandBus,
            SalesDbContext context, IRepository<CreditNote, Guid> creditNotesRepository, IRepository<FinancialTransaction, Guid> transactionRepository, ISequenceNumberService<CreditNote, Guid> sequenceNumberService, IPublishEndpoint bus, IMapper mapper)
        {
            
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _context = context;
            _creditNotesRepository = creditNotesRepository;
            _transactionRepository = transactionRepository;
            _sequenceNumberService = sequenceNumberService;
            _bus = bus;
            _mapper = mapper;
        }
        public async Task<ValidationResult> Handle(ValidateCreditNoteCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException("");

            var keysq = nameof(CreditNote) + orgId;

            try
            {
                await LockProvider<string>.WaitAsync(keysq, cancellationToken);
                var currentUser =await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = false },
                 cancellationToken);
                var creditNote = await _creditNotesRepository.Table.Where(c => c.Id == request.Id).Include(c=>c.CreditNoteItems).FirstOrDefaultAsync(cancellationToken);
                if(creditNote==null) 
                        {
                    LockProvider<string>.Release(keysq);
                    return new ValidationResult
                    { Errors = { new ValidationFailure("Erreur", "Avoir introuvable") } };
                };
                if (creditNote.State != Core.Shared.Enums.CreditNoteState.Draft)
                {
                    LockProvider<string>.Release(keysq);
                    return new ValidationResult { Errors = { new ValidationFailure("Erreur", "Avoir ne peut pas être validé") } };
                }
                creditNote.State = Core.Shared.Enums.CreditNoteState.Validated;
                creditNote.ValidatedOn = DateTime.Now;
                creditNote.ValidatedByUserId = currentUser.Id;


                    var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(creditNote.CreditNoteDate, orgId.Value);
                creditNote.SequenceNumber = sq;
                _creditNotesRepository.Update(creditNote);
                await _creditNotesRepository.UnitOfWork.SaveChangesAsync();
                #region Financial transaction insertion
                var transaction = new FinancialTransaction
                {
                    DocumentDate = creditNote.CreditNoteDate,
                    OrganizationId = orgId.Value,
                    RefDocument = "AV-" + creditNote.CreditNoteDate.Date.ToString("yy-MM-dd").Substring(0, 2)
                                       + "/" + "0000000000".Substring(0, 10 - creditNote.SequenceNumber.ToString().Length) + creditNote.SequenceNumber,
                    FinancialTransactionType = FinancialTransactionType.Refund,
                    CustomerId = creditNote.CustomerId,
                    CustomerName = creditNote.CustomerName,
                    SupplierId = creditNote.SupplierId,
                    RefNumber = creditNote.OrderNumber,
                    TransactionAmount = creditNote.TotalTTC
                };
                _transactionRepository.Add(transaction);
                #endregion
                
                await _transactionRepository.UnitOfWork.SaveChangesAsync();

                #region Inventoty Operations https://dev.azure.com/hydrapharmgroup/GHPCommerce/_sprints/taskboard/GHPCommerce%20Team/GHPCommerce/Sprint%2013?workitem=1115
                if(creditNote.ProductReturn)
                await _bus.Publish<ICreditNoteSubmittedEvent>(new
                {
                    CreditNoteId = creditNote.Id,
                    CorrelationId = Guid.NewGuid(),
                    ItemEvents = _mapper.Map<List<CreditNoteItemForEvent>>(creditNote.CreditNoteItems),
                    OrganizationId = creditNote.OrganizationId,
                    Userid = _currentUser.UserId,
                    RefDoc = "AV-" + creditNote.CreditNoteDate.Date.ToString("yy-MM-dd").Substring(0, 2)
                                       + "/" + "0000000000".Substring(0, 10 - creditNote.SequenceNumber.ToString().Length) + creditNote.SequenceNumber                                       

                }, cancellationToken);

                #endregion

                LockProvider<string>.Release(keysq);

            }
            catch (Exception e)
            {
                LockProvider<string>.Release(keysq);
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", e.Message) } };                
                
                return validations;
            }           
            return default;
        }
    }
}