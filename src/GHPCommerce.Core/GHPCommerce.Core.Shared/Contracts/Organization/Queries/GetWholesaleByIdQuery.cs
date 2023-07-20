using System;
using GHPCommerce.Core.Shared.Contracts.Organization.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Organization.Queries
{
    public class GetWholesaleByIdQuery :ICommand<WholeSalerDto>
    {
        public Guid SupplierOrganizationId { get; set; }
        public Guid CustomerOrganizationId { get; set; }
    }
}
