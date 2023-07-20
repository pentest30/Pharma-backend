using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Inventory.DTOs;

namespace GHPCommerce.Modules.Inventory.Queries
{
    public class GetPagedInventTransactionQuery : ICommand<SyncPagedResult<InventItemTransactionDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
}
