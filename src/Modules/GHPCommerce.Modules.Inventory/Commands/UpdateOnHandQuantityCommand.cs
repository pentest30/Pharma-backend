using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Inventory.Commands
{
    public class UpdateOnHandQuantityCommand : ICommand<ValidationResult>
    {
        public string InternalBatchNumber { get; set; }
        public double PhysicalOnHandQuantity { get; set; }
        public string ProductCode { get; set; }
        public int? OrderNumber { get; set; }
        public bool LineReserved { get; set; }
        public string Comment { get; set; }
        
    }
}
