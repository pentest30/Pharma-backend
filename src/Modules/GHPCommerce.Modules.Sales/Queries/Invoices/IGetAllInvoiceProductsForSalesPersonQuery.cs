using System;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public interface IGetAllInvoiceProductsForSalesPersonQuery
    {
        public DateTime? Start { get; set; }
        public DateTime?  End { get; set; }
    }
}