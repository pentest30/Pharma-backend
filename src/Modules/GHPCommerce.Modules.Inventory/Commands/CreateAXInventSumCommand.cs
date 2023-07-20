using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Inventory.Commands
{
    public class CreateAXInventSumCommand : ICommand<ValidationResult>
    {
        public string ProductCode { get; set; }
        public string InternalBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public double? PurchaseUnitPrice { get; set; }
        public float? PurchaseDiscountRatio { get; set; }
        public double? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        public double PhysicalOnHandQuantity { get; set; }
        public bool IsPublic { get; set; }
        public string PackagingCode { get; set; }
        public int packing { get; set; }

        public string VendorBatchNumber { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
    }
}
