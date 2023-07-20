using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.HumanResource.Entities;

namespace GHPCommerce.Modules.HumanResource.Commands
{
    public class CreateEmployeeCommand : ICommand<ValidationResult>
    {
        public Guid? Id { get; set; }
        public string HrCode { get; set; }
        public string JobTitle { get; set; }
        public string Name { get; set; }
        public EmployeeFunction Step { get; set; }
    }

    public class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, ValidationResult>
    {
        private readonly IRepository<Employee, Guid> _employeeRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly IMapper _mapper;

        public CreateEmployeeCommandHandler(IRepository<Employee, Guid> employeeRepository,
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

        public async Task<ValidationResult> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException("");
            ValidationResult validations = default;
            var employee = _mapper.Map<Employee>(request);
            employee.OrganizationId = orgId.Value;
            _employeeRepository.Add(employee);
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