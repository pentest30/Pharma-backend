using System.Collections.Generic;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;

namespace GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs
{
    public class PreparationOrderDtoV6
    {
        public string ZoneGroupName { get; set; }
        public List<PreparationOrderItemDtoV1> PreparationOrderItems { get; set; }
    }
}