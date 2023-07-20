using GHPCommerce.Domain.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GHPCommerce.Modules.Inventory.Entities
{
    public class Batch : AggregateRoot<Guid>
    {
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }

        [MaxLength(100)]
        public string OrganizationName { get; set; }

        [MaxLength(100)]
        public string ProductCode { get; set; }

        [MaxLength(100)]
        public string ProductFullName { get; set; }
        [MaxLength(100)]
        public string VendorBatchNumber { get; set; }

        [MaxLength(100)]
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
        public List<Invent> Invents { get; set; }

    }
}
