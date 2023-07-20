using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.ProductClasses.Commands
{
    public class DeleteProductClassCommand: ICommand
    {
        public Guid ProductClassId { get; set; }
        public Guid Id { get; set; }
    }
}
