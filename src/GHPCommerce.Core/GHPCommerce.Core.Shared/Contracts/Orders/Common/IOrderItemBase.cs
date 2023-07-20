namespace GHPCommerce.Core.Shared.Contracts.Orders.Common
{
    public interface IOrderItemBase
    {
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public double Discount { get; set; }
        public double Tax { get; set; }
    }
}