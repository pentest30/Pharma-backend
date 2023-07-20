using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Inventory.DTOs;

namespace GHPCommerce.Modules.Inventory.Queries
{
    public class GetPagedInventQuery : ICommand<SyncPagedResult<InventSumDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
}
