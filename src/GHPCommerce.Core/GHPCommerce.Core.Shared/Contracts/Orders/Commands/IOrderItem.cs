using System;
using GHPCommerce.Core.Shared.Enums;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Commands
{
    public interface IOrderItem
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public int OldQuantity { get; set; }
        public Guid SupplierOrganizationId { get; set; }
        public DateTime? MinExpiryDate { get; set; }
        public string InternalBatchNumber { get; set; }
        public Guid CustomerId { get; set; }
        public string ProductCode { get; set; }
        public string PackagingCode { get; set; }
        public decimal PFS { get; set; }
        public decimal PpaHT { get; set; }
        public decimal PpaTTC { get; set; }
        public decimal PpaPFS { get; set; }
        public OrderType OrderType { get; set; }
        public decimal ExtraDiscount { get; set; }
        public decimal Discount { get; set; }
        public string DocumentRef { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public string ZoneGroupName { get; set; }
        public int Packing { get; set; }
        public string DefaultLocation { get; set; }
        public int PickingZoneOrder { get; set; }
    }
}
