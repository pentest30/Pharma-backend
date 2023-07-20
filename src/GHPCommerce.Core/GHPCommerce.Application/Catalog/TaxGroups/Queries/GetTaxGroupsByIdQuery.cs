using System;
using GHPCommerce.Application.Catalog.TaxGroups.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TaxGroups.Queries
{
    public class GetTaxGroupsByIdQuery :ICommand<TaxGroupDto>
    {
        public Guid Id { get; set; }
    }
}
