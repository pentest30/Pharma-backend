using System.Collections.Generic;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;

namespace GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs
{
    public class AxValidationDto
    {
        public AxValidationDto()
        {
            OrderValidationErrors = new Dictionary<string, string>();
            InvalidItems = new List<InvalidPreparationOderItemDto>();
            
        }
        public Dictionary<string,string> OrderValidationErrors { get; set; }
        
        public List<InvalidPreparationOderItemDto> InvalidItems { get; set; }
        public bool IsValid { get; set; }
        public string CodeAx { get; set; }
    }
}