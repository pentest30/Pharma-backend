using System.Collections.Generic;
using GHPCommerce.Application.Catalog.INNs.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.INNs.Queries
{
    public class GetAllInnsQuery : ICommand<IEnumerable<InnDto>>
    {
    }
}
