using System;
using GHPCommerce.Core.Shared.Events.PreparationOrder;

namespace GHPCommerce.Core.Shared.PreparationOrder.DTOs
{
    public class PreparationOrderItemDtoV1
    {
        public Guid? OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public int? PackingQuantity { get; set; }
        public int Packing { get; set; }
        public string InternalBatchNumber { get; set; }
        public string PreviousInternalBatchNumber { get; set; }
        public string VendorBatchNumber { get; set; }

        public double Discount { get; set; }
        public double ExtraDiscount { get; set; }
        public Guid? PickingZoneId { get; set; }
        public int PickingZoneOrder { get; set; }
        public string PickingZoneName { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public string ZoneGroupName { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal PpaHT { get; set; }
        public string DefaultLocation { get; set; }
        public BlStatus Status { get; set; }
        public bool IsControlled { get; set; }
        public int? OldQuantity { get; set; }
        public int OrderNumberSequence { get; set; }
    }
}
