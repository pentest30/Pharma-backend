using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Catalog.Queries
{
    public class ActiveProductExistsByCode:ICommand<bool>
    {
        public string Code { get; set; }
    }
}
