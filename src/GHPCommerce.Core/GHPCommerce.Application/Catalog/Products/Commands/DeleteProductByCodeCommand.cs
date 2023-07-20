using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class DeleteProductByCodeCommand : ICommand
    {
        public string Code { get; set; }
        public Guid Id { get; set; }
    }
}
