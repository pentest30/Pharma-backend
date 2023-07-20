namespace GHPCommerce.Modules.Quota.DTOs
{
    public class CustomerQuotaDto
    {
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int InitQuantity { get; set; }
        public string SalesPersonName { get; set; }
        public string QuotaDate { get; set; }
        
    }
}