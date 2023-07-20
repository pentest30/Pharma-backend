using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Inventory.DTOs;

namespace GHPCommerce.Modules.Inventory.Queries
{
    public class GetInventSumByDimensionQuery : ICommand<InventSumDto>
    {
        public InventoryDimensionExistsQuery Dimension { get; set; }
    }
}