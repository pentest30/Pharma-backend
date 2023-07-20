using System.Collections.Generic;
using GHPCommerce.Application.Catalog.TaxGroups.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TaxGroups.Queries
{
    public class GetAllTaxesQuery :ICommand<IEnumerable<TaxGroupDto>>
    {
    }
}
