using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.INNs.Commands
{
    public class DeleteInnCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
