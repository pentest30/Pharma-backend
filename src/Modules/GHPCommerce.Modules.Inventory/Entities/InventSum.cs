using System;
using System.ComponentModel.DataAnnotations;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Inventory.Entities
{
    //Extraction de la table stock
   
    public class InventSum : AggregateRoot<Guid>, IEquatable<InventSum>
    {
        public Guid OrganizationId { get; set; }
        public Guid ProductId { get; set; }
        
       
        public string OrganizationName { get; set; }
        public string SupplierName { get; set; }

     
        public string ProductCode { get; set; }
        
       
        public string ProductFullName { get; set; }
      
        public string PackagingCode { get; set; }
        public int packing { get; set; }
        public bool Quota { get; set; }
        public string Manufacturer { get; set; }


        //[MaxLength(100)]
        //public string ProductInnCode { get; set; }

        #region  Batch
        [MaxLength(100)]
        public string VendorBatchNumber { get; set; }
        
        [MaxLength(100)]

        public string InternalBatchNumber { get; set; }
        public DateTime? ProductionDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        //In Dynamics AX DLUO for example =60 , expiry =31/12 =>BestBeforeDate=Date de fin de commercialisation =01/10
        public DateTime? BestBeforeDate { get; set; }
        #endregion

        #region Variant Management ToDo: We won't go in details now so we keep two variant types:Color,Size as string
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
        public double PhysicalOnhandQuantity { get; set; }
        
        public double PhysicalReservedQuantity { get; set; }//Calculated
        // quantité reservée aux quatos
        public double? PhysicalDispenseQuantity { get; set; }
        public double PhysicalAvailableQuantity //Not persisted
            => PhysicalOnhandQuantity-PhysicalReservedQuantity; //Calculated
        #endregion

        #region Status
        public bool IsPublic { get; set; }
        #endregion

        #region Store physical dimensions

        public Guid? SiteId { get; set; }
        [MaxLength(100)]
        public string SiteName { get; set; }
        public Guid? WarehouseId { get; set; }
        [MaxLength(100)]
        public string WarehouseName { get; set; }
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
        public  bool Equals(InventSum obj)
        {
            return obj?.Id == Id;
            // or: 
            // var o = (Person)obj;
            // return o.Id == Id && o.Name == Name;
        }
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
