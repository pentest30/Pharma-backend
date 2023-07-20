using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Packaging.Commands
{
    public class DeletePackagingCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
