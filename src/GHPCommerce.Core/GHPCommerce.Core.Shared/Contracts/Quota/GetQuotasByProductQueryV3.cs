using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Quota
{
    public class GetQuotasByProductQueryV3 : ICommand<Int32>
    {
        public Guid ProductId { get; set; }
        public Guid? SalesPeronId { get; set; }
    }
}