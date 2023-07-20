using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Brands.Commands
{
    public class DeleteBrandCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
