using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Batches.Queries
{
    public class GetInternalBatchNumberQuery : ICommand<object>
    {
        public string VendorBatchNumber { get; set; }
        public Guid SupplierId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime ExpiryDate { get; set; }
        public double PurchasePriceInclDiscount { get; set; }
        public double SellingPrice { get; set; }
        public decimal PpaPrice { get; set; }
        public decimal Shp { get; set; }
        public int Packing { get; set; }
    
    }
}