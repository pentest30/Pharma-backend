using System;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Dtos
{
    public class InventSumDtoV1
    {
        public Guid ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public string VendorBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? BestBeforeDate { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        public double TotalPhysicalAvailableQuantity { get; set; }
        public double Tax { get; set; }
        public string PackagingCode { get; set; }

    }
}
