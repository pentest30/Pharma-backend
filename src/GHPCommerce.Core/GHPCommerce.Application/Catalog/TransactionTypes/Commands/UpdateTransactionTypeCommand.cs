using FluentValidation;

namespace GHPCommerce.Application.Catalog.TransactionTypes.Commands
{
    public class UpdateTransactionTypeCommand : CreateTransactionTypeCommand
    {

    }
    public class UpdateTransactionTypeCommandValidator : AbstractValidator<UpdateTransactionTypeCommand>
    {
        public UpdateTransactionTypeCommandValidator()
        {

        }

    }
}
