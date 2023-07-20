using System;

namespace GHPCommerce.Core.Shared.Contracts.Tiers.Guest
{
    [Serializable]
    public class ShoppingCartItemDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public Guid ProductId { get; set; }
        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? OrganizationId { get; set; }
        public Guid CartId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string FullName { get; set; }
        public string Code { get; set; }
    }
}
