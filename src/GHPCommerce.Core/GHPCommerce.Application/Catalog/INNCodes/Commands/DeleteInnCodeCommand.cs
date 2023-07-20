using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.INNCodes.Commands
{
    public class DeleteInnCodeCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
