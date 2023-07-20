using System;

namespace GHPCommerce.Core.Shared.Contracts.Cache
{
    public class ValidateOnlineOrderContract
    {
        public string CustomerId { get; set; }
        public Guid OrderId { get; set; }
       
    }

    public class CancelOnlineOrderContract 
    {
        public string CustomerId { get; set; }
        public Guid OrderId { get; set; }
        public int Cancelation { get; set; } = 0;
    }
}