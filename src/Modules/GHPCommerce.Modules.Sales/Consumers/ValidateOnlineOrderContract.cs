using System;

namespace GHPCommerce.Modules.Sales.Consumers
{
    public class ValidateOnlineOrderContract
    {
        public string CustomerId { get; set; }
        public Guid OrderId { get; set; }
    }
}