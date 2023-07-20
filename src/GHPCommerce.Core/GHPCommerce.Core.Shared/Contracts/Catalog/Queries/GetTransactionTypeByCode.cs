using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class GetTransactionTypeByCode : ICommand<TransactionTypeDtoV1>
    {
        public TransactionTypeCode Code { get; set; }
    }
}
