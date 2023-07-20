using FluentValidation.Results;

namespace GHPCommerce.Core.Shared.Contracts.Inventory.Commands
{
    public  class ReserveDispatchedQuantityResponse
    {
        public ValidationResult ValidationResult { get; set; }
        public int RemainedQuantity { get; set; }
    }
}