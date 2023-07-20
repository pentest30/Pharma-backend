using System.Collections.Generic;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.INNCodes.Queries
{
    public class GetAllInnCodesQuery : ICommand<IEnumerable<InnCodeDto>>
    {
    }
}
