using GHPCommerce.Application.Catalog.TransactionTypes.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using System.Collections.Generic;

namespace GHPCommerce.Application.Catalog.TransactionTypes.Queries
{
    public class GetAllTransactionTypeQuery : ICommand<IEnumerable<TransactionTypeDto>>
    {
    }
}
