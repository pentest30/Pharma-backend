using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Application.Catalog.TransactionTypes.Commands
{
    public class CreateTransactionTypeCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
        public string TransactionTypeName { get; set; }
        public bool Blocked { get; set; }
        public TransactionTypeCode CodeTransaction { get; set; }

    }
    public class CreateTransactionTypeCommandValidator : AbstractValidator<CreateTransactionTypeCommand>
    {
        public CreateTransactionTypeCommandValidator()
        {

        }

    }
}
