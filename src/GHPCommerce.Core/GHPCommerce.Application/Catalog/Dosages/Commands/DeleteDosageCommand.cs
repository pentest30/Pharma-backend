using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Dosages.Commands
{
    public class DeleteDosageCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
