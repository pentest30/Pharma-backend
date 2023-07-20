using System;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class OrderItemUpdateCommandV2 : OrderItemCreateCommand
    {
        public Guid? CreatedByUserId { get; set; }
    }
}
