namespace GHPCommerce.Modules.Sales.Models
{
    public class DetailPendingOrder
    {
        public string SalesPersonName { get; set; }
        public string InternalBatchNumber { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public string OrderNumber { get; set; }
        
    }
}