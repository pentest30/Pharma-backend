using System;

namespace GHPCommerce.Core.Shared.Events.DeliveryReceipts
{
    public interface IDeliveryReceiptCompletedEvent
    {
        Guid DeliveryReceiptId { get; }
        Guid CorrelationId { get; }
        public Guid UserId { get; set; }
    }
}