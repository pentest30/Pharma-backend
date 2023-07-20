using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    /// <summary>
    /// this command is used for the creation of an order by a pharmacist
    /// </summary>
    public class CreateOrderByPharmacistCommand :ICommand<ValidationResult>
    {
        public Guid Id { get; set; } 
        public Guid SupplierId { get; set; } 
        
    } 
}
