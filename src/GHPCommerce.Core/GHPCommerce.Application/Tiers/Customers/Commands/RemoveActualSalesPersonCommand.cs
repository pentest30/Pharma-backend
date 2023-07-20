using System;
using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Tiers.Customers.Commands
{
    public class RemoveActualSalesPersonCommand : ICommand<ValidationResult>
    {
        public List<Guid> SalesPersonIds { get; set; }
    }
}