using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Events.DeliveryReceipts
{
    public interface IDeliveryReceiptSubmittedEvent
    {
        Guid DeliveryReceiptId { get; }
        Guid CorrelationId { get; }
        IList<DeliveryItem> ItemEvents { get; }
        public Guid OrganizationId { get; }
        public Guid UserId { get; set; }
        public string RefDoc { get; set; }
    }
}