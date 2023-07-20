using GHPCommerce.Domain.Domain.Common;
using System;

namespace GHPCommerce.Modules.PreparationOrder.Entities
{
    public class PreparationOrderItem : Entity<Guid>
    {
        public PreparationOrderItem()
        {
            CreatedDateTime = DateTimeOffset.Now;
        }
        public Guid PreparationOrderId { get; set; }

        public Guid? OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public int? OldQuantity { get; set; }
        public int? PackingQuantity { get; set; }
        public int Packing { get; set; }
        public string InternalBatchNumber { get; set; }
        
        public string VendorBatchNumber { get; set; }
        public double Discount { get; set; }
        public double OldDiscount { get; set; }
        public double ExtraDiscount { get; set; }
        public Guid? PickingZoneId { get; set; }
        public string PickingZoneName { get; set; }
        public Guid? ZoneGroupId { get; set; }
        public string ZoneGroupName { get; set; }
        public int PickingZoneOrder { get; set; }
        public string DefaultLocation { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal PpaHT { get; set; }
        public BlStatus Status { get; set; }
        public bool IsControlled { get; set; }
        // l'ancien lot de la ligne OP
        public string PreviousInternalBatchNumber { get; set; }

        public PreparationOrder PreparationOrder { get; set; }

    }
}
