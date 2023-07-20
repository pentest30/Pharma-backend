using System;

namespace GHPCommerce.Modules.Sales.DTOs.Discounts
{
    public class DiscountDtoV1
    {
        public Guid ProductId { get; set; }
        public decimal DiscountRate { get; set; }
        public int ThresholdQuantity { get; set; }

    }
}