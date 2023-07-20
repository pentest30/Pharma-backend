using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Commands
{
    public class DeletePharmacologicalClassCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
