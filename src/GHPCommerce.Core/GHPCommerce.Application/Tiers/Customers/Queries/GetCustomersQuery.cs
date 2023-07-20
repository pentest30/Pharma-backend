using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Customers.Queries
{
    public class GetCustomersQuery : CommonListQuery, ICommand<PagingResult<SupplierCustomerDto>>
    {
        public GetCustomersQuery(string term, string sort, int page, int pageSize)
             : base(term, sort, page, pageSize) { }
    }
}
