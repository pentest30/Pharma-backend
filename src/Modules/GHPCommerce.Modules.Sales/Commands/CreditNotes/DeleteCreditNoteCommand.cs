using System;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.CreditNotes
{
    public class DeleteCreditNoteCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }
    public class DeleteCreditNoteCommandValidator : AbstractValidator<DeleteCreditNoteCommand>
    {

        public DeleteCreditNoteCommandValidator()
        {

            RuleFor(v => v.Id).NotEmpty();

        }


    }
}