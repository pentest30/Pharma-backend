using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using System;

namespace GHPCommerce.Modules.PreparationOrder.Queries
{
    public class GetControlledBlByOrderQuery : ICommand<PreparationOrderDtoV3>
    {
        public Guid OrderId { get; set; }
        public Guid PickingZoneId { get; set; }
            
    }
}
