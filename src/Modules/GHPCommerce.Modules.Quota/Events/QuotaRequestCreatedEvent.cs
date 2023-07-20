using GHPCommerce.Domain.Domain.Events;

namespace GHPCommerce.Modules.Quota.Events
{
    public class QuotaRequestCreatedEvent : IEvent
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string SalesPersonName { get; set; }
    }
}