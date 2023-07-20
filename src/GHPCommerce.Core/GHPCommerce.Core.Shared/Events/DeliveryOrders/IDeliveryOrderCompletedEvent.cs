using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Events.DeliveryOrders
{
    public interface IDeliveryOrderCompletedEvent
    {
        Guid DeliveryOrderId { get; }
        Guid CorrelationId { get; }
        IList<DeliveryOrderItem> ItemEvents { get; }
        public Guid OrganizationId { get; }
        public Guid UserId { get; set; }
    }
}