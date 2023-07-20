using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.PreparationOrder.DTOs;

namespace GHPCommerce.Modules.PreparationOrder.Queries.Consolidation
{
    public class GetConsolidationOrderQuery: ICommand<ConsolidationValidationDto>
    {
        public string BarCode { get; set; }
    }
}