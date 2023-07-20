using System;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Dtos
{
    public class InventSumReservationDto
    {
        public Guid ProductId { get; set; }
        public Guid OrganizationId { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal? UnitPrice { get; set; }
        public double? Discount { get; set; }
        public double? ExtraDiscount { get; set; }

        public int Quantity { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public bool Error { get; set; }
        
         
    
         

    }
}