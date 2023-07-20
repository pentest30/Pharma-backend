using System;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Commands
{
    public class OrderCreateCommand : ICommand<CachedOrder>
    { 
        public CachedOrder OnlineOrderRequest { get; set; }
        public OrderCreateCommand(CachedOrder order)
        {
            OnlineOrderRequest = order;
            OrderId = order.Id;
        }
        public OrderCreateCommand()
        {

        }
        public Guid OrderId { get; set; }
       
        public Guid SupplierOrganizationId { get; set;}
    }

   
}