using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.HumanResource.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
namespace GHPCommerce.Modules.HumanResource.Queries
{
    public class GetEmployeePagedQuery : ICommand<SyncPagedResult<EmployeeDto>>
    { 
        public SyncDataGridQuery SyncDataGridQuery { get; set; }

    }
    public class GetEmployeePagedQueryHandler : ICommandHandler<GetEmployeePagedQuery, SyncPagedResult<EmployeeDto>>
    {
        private readonly IRepository<Entities.Employee, Guid> _employeeRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public GetEmployeePagedQueryHandler(IRepository<Entities.Employee, Guid> employeeRepository, 
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            ICommandBus commandBus)
        {
            _employeeRepository = employeeRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
        }
        public async Task<SyncPagedResult<EmployeeDto>> Handle(GetEmployeePagedQuery request, CancellationToken cancellationToken)
        {
            var org = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (!org.HasValue)
                return new SyncPagedResult<EmployeeDto>();
            var query = _employeeRepository.Table
                .Where(x => x.OrganizationId == org)
                .DynamicWhereQuery(request.SyncDataGridQuery);
            var total = await query.CountAsync(cancellationToken: cancellationToken);
            var result = await query
                .Paged(request.SyncDataGridQuery.Skip / request.SyncDataGridQuery.Take + 1,
                    request.SyncDataGridQuery.Take)
                .ToListAsync(cancellationToken);
            var data = _mapper.Map<List<EmployeeDto>>(result);
          
            return new SyncPagedResult<EmployeeDto>
                {Result = data, Count = total};
        }
        
    }
}