using System.Collections.Generic;

namespace GHPCommerce.Modules.PreparationOrder.DTOs
{
    public class PreparationOrderDtoV3
    {
        public int CountBlNotControlled { get; set; }
        
        public List<PreparationOrderItemDto> items { get; set; }


    }
}
