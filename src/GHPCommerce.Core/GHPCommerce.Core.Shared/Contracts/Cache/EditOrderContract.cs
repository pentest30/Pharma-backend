using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Cache
{
    public class EditOrderContract
    {
        public EditOrderContract()
        {
            EditOrderItems = new List<EditOrderItem>();
        }
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public string SupplierId { get; set; }
        public List<EditOrderItem> EditOrderItems { get; set; }
        public decimal? TotalNetAmount { get; set; }
        public decimal? DiscTotValue { get; set; }
    }

    public class EditOrderItem
    {
        public string BatchNumber { get; set; }
        public decimal? DiscValue { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal? LineAmount { get; set; }
        public decimal? UnitPrice { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
    }
}