using System.Collections.Generic;
using GHPCommerce.Core.Shared.Events.Orders;

namespace GHPCommerce.Core.Shared.Events.Orders
{
    public class Order
    {
        public List<OrderItem> OrderItems { get; set; }

    }
}