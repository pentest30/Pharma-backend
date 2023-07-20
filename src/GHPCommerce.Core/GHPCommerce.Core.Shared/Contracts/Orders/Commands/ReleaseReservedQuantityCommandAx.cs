using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Core.Shared.Contracts.Orders.Commands
{
    public class ReleaseReservedQuantityCommandAx : ICommand<ValidationResult>
    {
        public string ProductCode { get; set; }
        public string InternalBatchNumber { get; set; }
        public int OrderNumber { get; set; }
        public bool LineReserved { get; set; }
        public string Comment { get; set; }
    }
}