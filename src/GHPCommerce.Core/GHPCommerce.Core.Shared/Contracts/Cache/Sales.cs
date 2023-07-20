using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Contracts.Orders.Common;

namespace GHPCommerce.Core.Shared.Contracts.Cache
{
    [Serializable]
    public class CachedOrder
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public Guid? CustomerId { get; set; }
        public string Code { get; set; }
        public string RefDocumentHpcs { get; set; }
        public DateTime? DateDocumentHpcs { get; set; }
        public Guid CreatedByUserId { get; set; }
        public decimal OrderDiscount { get; set; }
        public decimal OrderTotal { get; set; }

        public string CustomerName { get; set; }
        public string SupplierName { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? ExpectedShippingDate { get; set; }
        public DateTime OrderDate { get; set; }
        public string RefDocument { get; set; }

        public DateTimeOffset CreatedDateTime { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        public DateTimeOffset? UpdatedDateTime { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public bool Psychotropic { get; set; }
        public bool ToBeRespected { get; set; }
        public bool IsSpecialOrder { get; set; }
        
        public CachedOrder()
        {
            OrderItems = new List<CachedOrderItem>();
        }
        public List<CachedOrderItem> OrderItems { get; set; }
        public string FcmToken { get; set; }
    }
    [Serializable]
    public class CachedOrderItem : IOrderItemBase
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string VendorBatchNumber { get; set; }                      
        public string InternalBatchNumber { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PurchaseUnitPrice { get; set; }

        public double Discount { get; set; }
        public double Tax { get; set; }
        public Guid InventSumId { get; set; }
        public string ProductCode { get; set; }
        public decimal ExtraDiscount { get; set; }
        public string PackagingCode { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public string ZoneGroupName { get; set; }
        public int Packing { get; set; }
        public bool Psychotropic { get; set; }
        public bool Thermolabile { get; set; }
        public string DefaultLocation { get; set; }
        public int PickingZoneOrder { get; set; }
        public DateTime? MinExpiryDate { get; set; }
        

    }
    [Serializable]
    public class CachedOrderList
    {
        public List<CachedOrder> OrderItems { get; set; }

    }
}
