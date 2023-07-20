using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Sectors.Queries
{
    public class GetSectorIdByCodeQuery :ICommand<Guid>
    {
        public string Code { get; set; }
    }
}
