using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Tiers.Suppliers.Queries
{
    public class GetPagedSuppliersQuery : ICommand<SyncPagedResult<SupplierDto>>
    {
        public SyncDataGridQuery GridQuery { get; set; }
    }
}
