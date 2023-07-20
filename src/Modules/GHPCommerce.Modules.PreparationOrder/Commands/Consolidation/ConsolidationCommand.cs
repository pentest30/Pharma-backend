using System;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.PreparationOrder.Commands.Consolidation
{
    public class ConsolidationCommand : ICommand<ValidationResult>
    {
        public Guid OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public Guid CustomerId { get; set; }

        public string CustomerName { get; set; }
        public Guid OrderId { get; set; }
        public DateTime? OrderDate { get; set; }

        public string OrderIdentifier { get; set; }
        public Guid ConsolidatedById { get; set; }
        public string ConsolidatedByName { get; set; }
        public string ReceivedInShippingByName { get; set; }
        public Guid? ReceivedInShippingById { get; set; }
        public int TotalPackage { get; set; }
        public int TotalPackageThermolabile { get; set; }
        public string EmployeeCode { get; set; }

    }
}
