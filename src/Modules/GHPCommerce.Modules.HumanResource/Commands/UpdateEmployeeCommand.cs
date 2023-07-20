using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.HumanResource.Entities;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace GHPCommerce.Modules.HumanResource.Commands
{
    public class UpdateEmployeeCommand : CreateEmployeeCommand
    {
        public class UpdateEmployeeCommandHandler : ICommandHandler<UpdateEmployeeCommand, ValidationResult>
        {
            private readonly IRepository<Employee, Guid> _employeeRepository;
            private readonly ICurrentOrganization _currentOrganization;
            private readonly ICommandBus _commandBus;
            private readonly ICurrentUser _currentUser;
            private readonly IMapper _mapper;

            public UpdateEmployeeCommandHandler(IRepository<Employee, Guid> employeeRepository,
                IMapper mapper,
                ICurrentOrganization currentOrganization,
                ICommandBus commandBus,
                ICurrentUser currentUser
            )
            {
                _employeeRepository = employeeRepository;
                _currentOrganization = currentOrganization;
                _commandBus = commandBus;
                _currentUser = currentUser;
                _mapper = mapper;
            }

            public async Task<ValidationResult> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
            {
                var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                if (orgId == default) throw new InvalidOperationException("");
                ValidationResult validations = default;
                var employee = await _employeeRepository.Table.Where(c => c.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                employee.Name = request.Name;
                employee.HrCode = request.HrCode;
                employee.JobTitle = request.JobTitle;
                employee.Step = request.Step;
                _employeeRepository.Update(employee);
                try
                {
                    await _employeeRepository.UnitOfWork.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    throw e;
                }

                return default;
            }
        }
    }

    class UpdateEmployeeCommandImpl : UpdateEmployeeCommand
    {
    }
}