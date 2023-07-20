using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Employees.DTOs;
using GHPCommerce.Core.Shared.Contracts.Employees.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using System.Linq;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Modules.HumanResource.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.HumanResource.Queries
{
    public class GetEmployeeByFunctionQueryHandler : ICommandHandler<GetEmployeeByFunctionQuery, List<EmployeeDto1>>
    {
        private readonly IRepository<Entities.Employee, Guid> _employeeRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetEmployeeByFunctionQueryHandler(IRepository<Entities.Employee, Guid> employeeRepository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            ICommandBus commandBus)
        {
            _employeeRepository = employeeRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }
        public async Task<List<EmployeeDto1>> Handle(GetEmployeeByFunctionQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new List<EmployeeDto1>();
            var result = await _employeeRepository.Table
                .Where(x => x.OrganizationId == org && x.Step == (EmployeeFunction)request.FunctionCode)
                .OrderBy(x => x.Name)
                .ToListAsync();
            return _mapper.Map<List<EmployeeDto1>>(result);
          
        }
        
    }
}