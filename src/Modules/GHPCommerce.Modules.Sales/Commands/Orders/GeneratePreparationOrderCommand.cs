using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class GeneratePreparationOrderCommand : ICommand<ValidationResult>
    {
        public Guid OrderId { get; set; }
       
    }
}
