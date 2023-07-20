using System;

namespace GHPCommerce.Modules.Inventory.DTOs.Batches
{
    public class BatchDto
    {
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public string OrganizationName { get; set; }
        public string ProductCode { get; set; }
        public string ProductFullName { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public double? PurchaseUnitPrice { get; set; }
        public float? PurchaseDiscountRatio { get; set; }
        public double? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        public int packing { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string ExpiryDateShort { get; set; }
        public string CreatedBy { get; set; }
        public DateTimeOffset CreatedDateTime { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}