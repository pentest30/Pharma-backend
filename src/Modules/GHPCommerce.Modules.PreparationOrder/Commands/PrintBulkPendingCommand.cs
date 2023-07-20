using GHPCommerce.Domain.Domain.Commands;
using FluentValidation.Results;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class PrintBulkPendingCommand : ICommand<ValidationResult>
    {
        public string CustomerName { get; set; }
        public string OrganizationName { get; set; }
        public string ZoneGroupName { get; set; }
        public string SectorName { get; set; }
    }
}
