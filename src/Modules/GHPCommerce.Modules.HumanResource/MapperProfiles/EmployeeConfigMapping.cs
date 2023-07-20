using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Employees.DTOs;
using GHPCommerce.Modules.HumanResource.Commands;
using GHPCommerce.Modules.HumanResource.DTOs;
using GHPCommerce.Modules.HumanResource.Entities;

namespace GHPCommerce.Modules.HumanResource.MapperProfiles
{
    public class EmployeeConfigMapping: Profile
    {
        public EmployeeConfigMapping()
        {
            CreateMap<CreateEmployeeCommand,Employee>().ReverseMap();
            CreateMap<Employee  , EmployeeDto>().ReverseMap();
            CreateMap<Employee  , EmployeeDto1>().ReverseMap();

        }
    }
}