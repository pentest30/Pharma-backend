using System.Collections.Generic;
using GHPCommerce.Application.Catalog.PharmacologicalClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Queries
{
    public class GetAllPharmacologicalClassesQuery : ICommand<IEnumerable<PharmacologicalClassDto>>
    {
    }
}
