using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Events.Orders;
using GHPCommerce.Core.Shared.Events.PreparationOrder;

namespace GHPCommerce.Core.Shared.Events.DeliveryOrders
{
    public interface IDeliveryOrderSubmittedEvent
    {
        Guid DeliveryOrderId { get; }
        Guid CorrelationId { get; }
        IList<DeliveryOrderItem> ItemEvents { get; }
        public Guid OrganizationId { get; }
        public Guid UserId { get; set; }
        public string RefDoc { get; set; }
        public DeliveryOrder DeliveryOrder { get; set; }
        public Order Order { get; set; }
        public List<PreparationOrderItem> OpItems { get; set; }
    }
}