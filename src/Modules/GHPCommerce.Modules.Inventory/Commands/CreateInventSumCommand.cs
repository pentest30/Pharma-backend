using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Inventory.Commands
{
    [Serializable]
    public class CreateInventSumCommand :ICommand<ValidationResult>
    {
        public Guid ProductId { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? BestBeforeDate { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public double? PurchaseUnitPrice { get; set; }
        public float? PurchaseDiscountRatio { get; set; }
        public double? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        public double PhysicalOnhandQuantity { get; set; }
        public double PhysicalReservedQuantity { get; set; }
        public bool IsPublic { get; set; }
        public Guid? SiteId { get; set; }
        public string SiteName { get; set; }
        public Guid? WarehouseId { get; set; }
        public string WarehouseName { get; set; }
        public double? MinThresholdAlert { get; set; }
        public string PackagingCode { get; set; }
        public int packing { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
    }
}