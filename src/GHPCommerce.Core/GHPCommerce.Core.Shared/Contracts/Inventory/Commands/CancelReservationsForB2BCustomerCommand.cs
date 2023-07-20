using System.Collections.Generic;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Inventory.Dtos;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public class CancelReservationsForB2BCustomerCommand : ICommand<ValidationResult>
    {
        public List<InventSumReservationDto> Reservations { get; set; }
         
    }
}