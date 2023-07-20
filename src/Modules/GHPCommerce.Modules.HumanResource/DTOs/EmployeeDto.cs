using System;
using GHPCommerce.Modules.HumanResource.Entities;

namespace GHPCommerce.Modules.HumanResource.DTOs
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public string HrCode { get; set; }
        public string JobTitle { get; set; }
        public string Name { get; set; }
        public EmployeeFunction Step { get; set; }
        public Guid OrganizationId { get; set; }    
    }
}