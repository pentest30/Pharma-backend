using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Quota
{
    public class GetQuotasByProductQueryV2 : ICommand<int>
    {
        public Guid ProductId { get; set; }
    }
}