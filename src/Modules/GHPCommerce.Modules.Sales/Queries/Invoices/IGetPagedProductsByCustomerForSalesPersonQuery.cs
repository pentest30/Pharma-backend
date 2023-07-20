using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public interface IGetPagedProductsByCustomerForSalesPersonQuery : IGetAllProductsByCustomerForSalesPersonQuery
    {
       
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
}