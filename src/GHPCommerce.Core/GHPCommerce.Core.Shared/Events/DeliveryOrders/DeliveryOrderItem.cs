using GHPCommerce.Core.Shared.Events.DeliveryReceipts;

namespace GHPCommerce.Core.Shared.Events.DeliveryOrders
{
    public class DeliveryOrderItem:DeliveryItem
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string VendorBatchNumber { get; set; }
    }
}