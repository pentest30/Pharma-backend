using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Inventory.Commands
{
    public class ActivateBlockInventSumCommand:ICommand<ValidationResult>
    {
        public string InternalBatchNumber { get; set; }
        public string ProductCode { get; set; }
        public bool IsPublic { get; set; }

    }
}
