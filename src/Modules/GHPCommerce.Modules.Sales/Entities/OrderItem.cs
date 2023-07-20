﻿using System;
using GHPCommerce.Core.Shared.Contracts.Orders.Common;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.Sales.Entities
{
    public class OrderItem : Entity<Guid>, IOrderItemBase
    {
        public OrderItem()
        {
            CreatedDateTime = DateTimeOffset.Now;
           // AcceptedOnAx = true;
        }

        public Guid? OrderId { get; set; }      
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public double ExtraDiscount { get; set; }

        public double Tax { get; set; }
        public decimal UnitPriceInclTax { get; set; }
        public string VendorBatchNumber { get; set; }
        public string InternalBatchNumber { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public virtual  Order Order { get; set; }
        public decimal TotalInlTax { get; set; }
        public decimal TotalExlTax { get; set; }
        public string PackagingCode { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public string ZoneGroupName { get; set; }
        public int Packing { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public decimal PurchaseUnitPrice { get; set; }
        public bool Thermolabile { get; set; }
        public string DefaultLocation { get; set; }
        public int? PickingZoneOrder { get; set; }
        public bool AcceptedOnAx { get; set; }
        public string Comment { get; set; }
        public void UpdateProductNameAndCode(string productName, string code)
        {
            ProductName = productName;
            ProductCode = code;
        }
    }
}
