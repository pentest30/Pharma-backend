using System;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public interface IGetAllProductsByCustomerForSalesPersonQuery :IGetAllInvoiceProductsForSalesPersonQuery
    {
        public Guid? CustomerId { get; set; }
    }
}