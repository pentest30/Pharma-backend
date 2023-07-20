using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.PreparationOrder.DTOs
{
    public class InvalidPreparationOderItemDto
    {
        public InvalidPreparationOderItemDto()
        {
            OrderItemValidationErrors = new Dictionary<string, string>();
        }
        public string InternalBatchNumber { get; set; }
        public string ProductCode { get; set; }
        
        public decimal Quantity { get; set; }
        public Dictionary<string,string> OrderItemValidationErrors { get; set; }
    }
}