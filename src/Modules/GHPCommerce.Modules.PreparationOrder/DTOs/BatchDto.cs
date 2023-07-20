using System;

namespace GHPCommerce.Modules.PreparationOrder.DTOs
{
    public class  BatchDto
    {
        public string InternalBatchNumber { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        
    }
}