using System;
using GHPCommerce.Application.Catalog.Manufacturers.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Manufacturers.Queries
{
    public class GetManufacturerByIdQuery : ICommand<ManufacturerDto>
    {
        public Guid Id { get; set; }
    }
}
