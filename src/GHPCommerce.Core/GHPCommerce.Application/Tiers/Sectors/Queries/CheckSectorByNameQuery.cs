using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Sectors.Queries
{
    public class CheckSectorByNameQuery : ICommand<bool>
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
