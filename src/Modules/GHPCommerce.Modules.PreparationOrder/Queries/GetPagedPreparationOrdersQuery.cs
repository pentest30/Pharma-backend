using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.PreparationOrder.DTOs;

namespace GHPCommerce.Modules.PreparationOrder.Queries
{
    public class GetPagedPreparationOrdersQuery : ICommand<SyncPagedResult<PreparationOrdersDto>>
    {
        public string barCode { get; set; }

        public SyncDataGridQuery DataGridQuery { get; set; }
    }
}
