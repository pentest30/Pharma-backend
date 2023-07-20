using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Quota.DTOs;

namespace GHPCommerce.Modules.Quota.Queries
{
    public class GetQuotaByCustomerQuery : ICommand<IEnumerable<QuotaDtoV1>>
    {
        //public Guid CustomerId { get; set; }
        
    }
}