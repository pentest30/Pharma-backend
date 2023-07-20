using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Events;

namespace GHPCommerce.Core.Shared.Contracts.Inventory
{
    [Serializable]
    public class InventSumCreatedEvent : IEvent,ICloneable
    {
        public InventSumCreatedEvent()
        {
            CachedInventSumCollection = new CachedInventSumCollection();
        }
        public string Id { get; set; }
        /*
         * Lists of objects must be encapsulated in on object
         */
        public CachedInventSumCollection CachedInventSumCollection { get; set; }


        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    [Serializable]
    public class CachedInventSumCollection : ICloneable
    {
        public CachedInventSumCollection()
        {
            CachedInventSums = new List<CachedInventSum>();
        }
        public List<CachedInventSum> CachedInventSums { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    [Serializable]
    public class CachedInventSum : ICloneable
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }


        public string OrganizationName { get; set; }
        public string SupplierName { get; set; }


        public string ProductCode { get; set; }


        public string ProductFullName { get; set; }

        //[MaxLength(100)]
        //public string ProductInnCode { get; set; }

        #region  Batch

        public string VendorBatchNumber { get; set; }



        public string InternalBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        //In Dynamics AX DLUO for example =60 , expiry =31/12 =>BestBeforeDate=Date de fin de commercialisation =01/10
        public DateTime? BestBeforeDate { get; set; }
        #endregion

        #region Variant Management ToDo: We won't go in details now so we keep two variant types:Color,Size as string

        public string Color { get; set; }

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
        public double PhysicalOnhandQuantity { get; set; }

        public double PhysicalReservedQuantity { get; set; }//Calculated
        public double PhysicalAvailableQuantity //Not persisted
        {
            get => PhysicalOnhandQuantity - PhysicalReservedQuantity;
        }//Calculated
        #endregion

        #region Status
        public bool IsPublic { get; set; }
        #endregion

        #region Store physical dimensions

        public Guid? SiteId { get; set; }

        public string SiteName { get; set; }
        public Guid? WarehouseId { get; set; }

        public string WarehouseName { get; set; }
        #endregion

        #region Thresholds

        //public double? MinThreshold { get; set; }
        public double? MinThresholdAlert { get; set; }
        public bool Error { get; set; }
        //public double? MaxThreshold { get; set; }
        //public double? MaxThresholdAlert { get; set; }
        #endregion

        public decimal ExtraDiscount { get; set; }
        public decimal PFS { get; set; }

        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }
        public string PackagingCode { get; set; }
        public int Packing { get; set; }

    }

}
