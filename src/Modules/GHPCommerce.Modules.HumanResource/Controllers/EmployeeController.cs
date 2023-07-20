using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Employees.DTOs;
using GHPCommerce.Core.Shared.Contracts.Employees.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.HumanResource.Commands;
using GHPCommerce.Modules.HumanResource.DTOs;
using GHPCommerce.Modules.HumanResource.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.HumanResource.Controllers
{
    [Route("api/employees/")]
    [ApiController]
    public class EmployeeController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public EmployeeController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Hr, PermissionAction.GET, "Admin", "SuperAdmin","Controller", "Consolidator", "Executer")]
        [Route("/api/employees/search")]
        public Task<SyncPagedResult<EmployeeDto>> GetEmployeeSync(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetEmployeePagedQuery { SyncDataGridQuery = query });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Hr, PermissionAction.GET, "Admin", "SuperAdmin","Controller", "Consolidator", "Executer")]
        [Route("/api/employees/{functionCode:int}/all")]
        public Task<List<EmployeeDto1>> GetEmployeeByFunction(int functionCode)
        {
            return _commandBus.SendAsync(new GetEmployeeByFunctionQuery { FunctionCode = functionCode });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/employees")]
        [ResourceAuthorization(PermissionItem.Hr, PermissionAction.POST, "Admin", "SuperAdmin","Controller", "Consolidator", "Executer")]
        public async Task<ActionResult> Create(CreateEmployeeCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/employees/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Hr, PermissionAction.POST, "Admin", "SuperAdmin","Controller", "Consolidator", "Executer")]
        public async Task<ActionResult> Create(UpdateEmployeeCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }

    }
}