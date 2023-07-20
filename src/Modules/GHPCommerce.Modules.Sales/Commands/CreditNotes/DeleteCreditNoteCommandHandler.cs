using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results; 

using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;  
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Modules.Sales.Commands.CreditNotes
{
    public class DeleteCreditNoteCommandHandler : ICommandHandler<DeleteCreditNoteCommand, ValidationResult>
    {
         
        private readonly IRepository<CreditNote, Guid> _repository;
        private  SalesDbContext _context;

        public DeleteCreditNoteCommandHandler(IRepository<CreditNote, Guid> repository, SalesDbContext context)
        { 
            _repository = repository;
            _context = context;

        }

        public async Task<ValidationResult> Handle(DeleteCreditNoteCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var creditNote =
                    await _repository.Table.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken); 
                if (creditNote == null) return new ValidationResult { Errors = { new ValidationFailure("Erreur", "Avoir introuvable") } };
                if (creditNote.State != Core.Shared.Enums.CreditNoteState.Draft) return new ValidationResult { Errors = { new ValidationFailure("Erreur", "Avoir ne peut pas être supprimé") } };

                _repository.Delete(creditNote);
                await _repository.UnitOfWork.SaveChangesAsync();
                
            }
            catch (Exception e)
            {
                throw e;
            }

            return default;
        }
    }
}