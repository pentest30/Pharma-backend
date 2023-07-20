using GHPCommerce.Domain.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GHPCommerce.Modules.Inventory.Entities
{
    public class Invent : AggregateRoot<Guid>
    {
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        public Guid BatchId { get; set; }
        public Guid ZoneId { get; set; }
        public string ZoneName { get; set; }
        public Guid StockStateId { get; set; }
        public string StockStateName { get; set; }

        [MaxLength(100)]
        public string OrganizationName { get; set; }

        [MaxLength(100)]
        public string ProductCode { get; set; }

        [MaxLength(100)]
        public string ProductFullName { get; set; }
        [MaxLength(100)]
        public string PackagingCode { get; set; }
        public int packing { get; set; }

        #region  Batch
        [MaxLength(100)]
        public string VendorBatchNumber { get; set; }

        [MaxLength(100)]

        public string InternalBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        //In Dynamics AX DLUO for example =60 , expiry =31/12 =>BestBeforeDate=Date de fin de commercialisation =01/10
        public DateTime? BestBeforeDate { get; set; }
        [MaxLength(100)]
        public string Color { get; set; }
        [MaxLength(100)]
        public string Size { get; set; }
        #endregion

        #region Inventory Valuation
        public double? PurchaseUnitPrice { get; set; }
        public float? PurchaseDiscountRatio { get; set; }
        #endregion
        #region Sales Pricing
        public double? SalesUnitPrice { get; set; }
        public float? SalesDiscountRatio { get; set; }
        #endregion

        #region Onhand Quantity
        public double PhysicalQuantity { get; set; }

        public double PhysicalReservedQuantity { get; set; }//Calculated
      

        #endregion

        #region Thresholds

        //public double? MinThreshold { get; set; }
        public double? MinThresholdAlert { get; set; }
        //public double? MaxThreshold { get; set; }
        //public double? MaxThresholdAlert { get; set; }
        #endregion
        /// <summary>
        /// supplément honoraire pharmacien (SHP)
        /// </summary>
        public decimal PFS { get; set; }

        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public List<InventItemTransaction> InventItemTransactions { get; set; }
        public Batch Batch { get; set; }
        public string SupplierName { get; set; }
        public Guid SupplierId { get; set; }

    }
}