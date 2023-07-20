using System;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Sales.DTOs;

namespace GHPCommerce.Modules.Sales.Queries
{
    public class GetOrdersListQuery : CommonListQuery, ICommand<PagingResult<OrderDto>>
    {
        public GetOrdersListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {
            
        }

        public Guid? CurrentCustomerId { get; set; }
    }
}
