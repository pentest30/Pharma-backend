using System.Collections.Generic;
using GHPCommerce.Application.Catalog.Manufacturers.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Manufacturers.Queries
{
    public class GetAllManufacturersQuery :ICommand<IEnumerable<ManufacturerDto>>
    {
    }
}
