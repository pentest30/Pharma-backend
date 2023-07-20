using System;
using System.Collections.Generic;
using GHPCommerce.Core.Shared.Events.Orders;
using GHPCommerce.Core.Shared.Events.PreparationOrder;

namespace GHPCommerce.Core.Shared.Events.DeliveryOrders
{
    public class InventoryDecreaseMessage
    {
        public Guid DeliveryOrderId { get; set; }
        public Guid CorrelationId { get; set; }

        public Guid UserId { get; set; }
        public string RefDoc { get; set; }
        public Guid OrganizationId { get; set; }
        public DeliveryOrder DeliveryOrder { get; set; }
        public Order Order { get; set; }
        public List<PreparationOrderItem> OpItems { get; set; }
    }
}