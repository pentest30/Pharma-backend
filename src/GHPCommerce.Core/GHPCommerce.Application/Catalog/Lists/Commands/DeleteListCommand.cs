using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Lists.Commands
{
    public class DeleteListCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
