using GHPCommerce.Domain.Domain.Commands;
using System;

namespace GHPCommerce.Application.Catalog.TransactionTypes.Commands
{
    public class DeleteTransactionTypeCommand : ICommand
    {
        public Guid Id { get; set; }

    }
}
