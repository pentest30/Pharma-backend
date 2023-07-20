using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Sectors.Commands
{
    public class DeleteSectorCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
