using System;

namespace GHPCommerce.Modules.Sales.Models
{
    public class OrderItemModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid CustomerId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int Quantity { get; set; }
        public Guid? SupplierOrganizationId { get; set; }
        public DateTime? MinExpiryDate { get; set; }
        public string InternalBatchNumber { get; set; }
        
    } 
}