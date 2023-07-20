using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Events.DeliveryReceipts
{
    public class InventoryMessage 
    {
        public InventoryMessage()
        {
            Items = new List<DeliveryItem>();
        }
        public Guid DeliveryReceiptId { get; set; }
        public Guid CorrelationId { get; set; }
        public List<DeliveryItem> Items { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; }
        public string RefDoc { get; set; }
    }
}