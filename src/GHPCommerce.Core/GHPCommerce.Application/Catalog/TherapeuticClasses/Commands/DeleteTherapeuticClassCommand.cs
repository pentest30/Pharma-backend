using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Commands
{
    public class DeleteTherapeuticClassCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
