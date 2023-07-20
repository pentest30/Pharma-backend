using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class DeleteProductCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
