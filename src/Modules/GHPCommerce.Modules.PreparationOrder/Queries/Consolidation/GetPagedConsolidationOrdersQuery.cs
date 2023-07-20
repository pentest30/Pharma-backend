using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.PreparationOrder.DTOs;

namespace GHPCommerce.Modules.PreparationOrder.Queries.Consolidation
{
    public class GetPagedConsolidationOrdersQuery : ICommand<SyncPagedResult<ConsolidationOrdersDto>>
    {
        public SyncDataGridQuery DataGridQuery { get; set; }
        public string barCode { get; set; }

    }
}
