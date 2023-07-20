using System.Collections.Generic;
using GHPCommerce.Application.Catalog.Dosages.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Dosages.Queries
{
    public class GetAllDosagesQuery :ICommand<IEnumerable<DosageDto>>
    {
    }
}
