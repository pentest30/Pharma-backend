using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Customers.Commands;
using GHPCommerce.Application.Tiers.Organizations.Commands;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Application.Tiers.Organizations.Queries;
using GHPCommerce.Core.Shared.Contracts.Organization.DTOs;
using GHPCommerce.Core.Shared.Contracts.Organization.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.Organizations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/organizations")]
    [ApiController]
    [Authorize]
    public class OrganizationsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public OrganizationsController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SuperAdmin")]
        public Task<PagingResult<OrganizationDtoV1>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetOrganizationListQuery(term, sort, page, pageSize));
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SuperAdmin")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/organizations/search")]
        [HttpPost]
        public async Task<SyncPagedResult<OrganizationDtoV1>> GetOrganizations([FromBody] SyncDataGridQuery query)
        {
            var q = Request;
            return await _commandBus.SendAsync(new PagedOrganizationsQuery { GridQuery = query });
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.POST, "SuperAdmin", "Admin")]
        public async Task<ActionResult> Create(OrganizationModel model)
        {
            var createCommand = _mapper.Map<CreateOrganizationCommand>(model);
            var result = await _commandBus.SendAsync(createCommand);
            return ApiCustomResponse(result);
        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/organizations/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.PUT, "SuperAdmin")]
        public async Task<ActionResult> Put([FromRoute] Guid id, OrganizationModel model)
        {
            // ReSharper disable once ComplexConditionExpression
            if (id == Guid.Empty || model.Id != id)
                return BadRequest();
            var createCommand = _mapper.Map<UpdateOrganizationCommand>(model);
            var result = await _commandBus.SendAsync(createCommand);
            return ApiCustomResponse(result);
        }


        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/Organizations/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SuperAdmin", "Admin")]
        public Task<IEnumerable<OrganizationDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllOrganizationsQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/Organizations/pharmacists")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public Task<IEnumerable<PharmacistDto>> GetPharmacists()
        {
            return _commandBus.SendAsync(new GetPharmacistQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/Organizations/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SuperAdmin", "OnlineCustomer")]
        public Task<OrganizationDtoV2> GetOrganizationById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Organization Id should not be null nor empty");
            return _commandBus.SendAsync(new GetOrganizationByIdQuery {Id = id});

        }

        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/organizations/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.DELETE, "SuperAdmin")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();
            try
            {
                await _commandBus.Send(new DeleteOrganizationCommand {Id = id});
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/organizations/{id:Guid}/activate")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.PUT, "SuperAdmin")]
        public async Task<ActionResult> Activate([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();
            try
            {
                await _commandBus.Send(new ActivateOrganizationCommand {Id = id});
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/organizations/ax")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.POST,  "Admin")]
        public async Task<ActionResult> Create(CreateAXOrganizationCommand model)
        {
            
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/organizations/{code}/ax")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.PUT, "Admin")]
        public async Task<ActionResult> Create([FromRoute] string code, UpdateAxCustomerCommand model)
        {
            if (code != model.Code)
                return BadRequest();
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/organizations/{code}/debt/ax")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.PUT, "Admin")]
        public async Task<ActionResult> Create([FromRoute] string code, UpdateAxCustomerDebtCommand model)
        {
            if (code != model.Code)
                return BadRequest();
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        
    }
}
