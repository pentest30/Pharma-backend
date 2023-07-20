using GHPCommerce.Domain.Domain.Commands;
using System;
using FluentValidation.Results;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class ZoneConsolidationCommand : ICommand<ValidationResult>
    {
        public Guid PreparationOrderId { get; set; }
        public Guid ConsolidatedById { get; set; }
        public string ConsolidatedByName { get; set; }
        public Guid ZoneGroupId { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public string EmployeeCode { get; set; }
    }
}
