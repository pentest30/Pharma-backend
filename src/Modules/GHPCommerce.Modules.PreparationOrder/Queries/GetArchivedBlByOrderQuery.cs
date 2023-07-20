using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.PreparationOrder.DTOs;

namespace GHPCommerce.Modules.PreparationOrder.Queries
{
    public class GetArchivedBlByOrderQuery : ICommand<SyncPagedResult<PreparationOrderDtoV4>>
    {
        public string barCode { get; set; }

        public SyncDataGridQuery DataGridQuery { get; set; }
    }
}
