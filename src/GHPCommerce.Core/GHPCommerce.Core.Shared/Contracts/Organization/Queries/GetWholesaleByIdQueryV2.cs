using System;
using GHPCommerce.Core.Shared.Contracts.Organization.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Organization.Queries
{
    public class GetWholesaleByIdQueryV2 : ICommand<WholeSalerDto>
    {
        public Guid SupplierOrganizationId { get; set; }
    }
}
