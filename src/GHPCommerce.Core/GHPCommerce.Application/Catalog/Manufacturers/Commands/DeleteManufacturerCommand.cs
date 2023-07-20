using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Manufacturers.Commands
{
    public class DeleteManufacturerCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
