using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Quota
{
    public class GetQuotasForSalesPersonByProductIdQuery : ICommand<Int32>
    {
        public Guid ProductId { get; set; }
        public Guid SalesPersonId { get; set; }
        public Guid CustomerId { get; set; }
        public int Quantity { get; set; }
    }
}