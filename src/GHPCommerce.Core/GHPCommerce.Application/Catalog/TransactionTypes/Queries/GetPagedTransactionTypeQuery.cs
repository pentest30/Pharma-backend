using GHPCommerce.Application.Catalog.TransactionTypes.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;


namespace GHPCommerce.Application.Catalog.TransactionTypes.Queries
{
    public class GetPagedTransactionTypeQuery : ICommand<SyncPagedResult<TransactionTypeDto>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }

    }
}
