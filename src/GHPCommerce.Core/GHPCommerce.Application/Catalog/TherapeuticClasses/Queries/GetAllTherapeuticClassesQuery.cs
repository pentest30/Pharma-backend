using System.Collections.Generic;
using GHPCommerce.Application.Catalog.TherapeuticClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Queries
{
    public class GetAllTherapeuticClassesQuery : ICommand<IEnumerable<TherapeuticClassDto>>
    {
    }
}
