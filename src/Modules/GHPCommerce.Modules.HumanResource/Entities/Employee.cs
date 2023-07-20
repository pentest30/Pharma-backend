using System;
using GHPCommerce.Domain.Domain.Common;

namespace GHPCommerce.Modules.HumanResource.Entities
{
    public class Employee : AggregateRoot<Guid>
    {
        public string HrCode { get; set; }
        public string JobTitle { get; set; }
        public string Name { get; set; }
        public EmployeeFunction Step { get; set; }
        public Guid OrganizationId { get; set; }
    }
}