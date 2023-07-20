using System;

namespace GHPCommerce.Core.Shared.Contracts.Batches.Dtos
{
    public class BatchDtoV1
    {
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
    }
}