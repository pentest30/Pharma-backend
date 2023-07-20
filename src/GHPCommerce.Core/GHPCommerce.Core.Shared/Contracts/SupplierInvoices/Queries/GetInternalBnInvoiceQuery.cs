using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.SupplierInvoices.Queries
{
    public class GetInternalBnInvoiceQuery : ICommand<int>
    {
        public string VendorBatchNumber { get; set; }
        public Guid SupplierId { get; set; }
        public Guid ProductId { get; set; }
    }
}