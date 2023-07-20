using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.TaxGroups.Commands
{
    public class DeleteTaxGroupCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
