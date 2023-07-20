using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Events.DeliveryReceipts
{
    public interface IdeliverReceiptCancelledEvent
    {
        Guid DeliveryReceiptId { get; }
        Guid CorrelationId { get; }
        IList<DeliveryItem> ItemEvents { get; }
        public Guid OrganizationId { get; set; }
        public Guid UserId { get; set; }
    }
}