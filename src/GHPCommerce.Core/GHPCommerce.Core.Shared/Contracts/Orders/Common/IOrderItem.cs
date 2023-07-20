using System;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Common
{
    internal interface IOrderItem: IOrderItemBase
    {
        public Guid? OrderId { get; set; }
        public Guid ProductId { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public int OldQuantity { get; set; }
        public Guid SupplierOrganizationId { get; set; }
        public DateTime? MinExpiryDate { get; set; }
    }

}
