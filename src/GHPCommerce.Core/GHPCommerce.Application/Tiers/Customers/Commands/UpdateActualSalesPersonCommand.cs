using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Customers.Commands
{
    public class UpdateActualSalesPersonCommand : ICommand<ValidationResult>
    {
        public Guid SalespersonsSourceId { get; set; }
        public Guid SalespersonsDestinationId { get; set; }
        public List<Guid> Customers { get; set; }
        public string SalespersonsDestination { get; set; }
    }
}