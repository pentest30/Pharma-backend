using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Employees.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;

namespace GHPCommerce.Core.Shared.Contracts.Employees.Queries
{
    public class GetEmployeeByFunctionQuery :  ICommand<List<EmployeeDto1>>
    {
        public int FunctionCode { get; set; }
    }
  
}