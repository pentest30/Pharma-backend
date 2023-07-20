using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using GHPCommerce.Application.Membership.Roles.Commands;
using GHPCommerce.Application.Membership.Roles.DTOs;
using GHPCommerce.Application.Membership.Roles.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
       
        private readonly IMapper _mapper;
       
        private readonly ICommandBus _commandBus;
        public RolesController( IMapper mapper,ICommandBus commandBus)
        {
          _mapper = mapper;
            _commandBus = commandBus;
        }
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "SuperAdmin", "Admin")]

        public Task<PagingResult<RoleDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetRoleListQuery(term, sort, page, pageSize));
        }
        [HttpGet]
        [Route("/api/roles/getAll")]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.GET, "SuperAdmin", "Admin")]

        public async Task<IEnumerable<RoleModel>> GetAll()
        {
            string[] excludedRoles = { };
            if (!User.IsInRole("SuperAdmin"))
                excludedRoles = new[] {"Admin", "SuperAdmin" ,"Group"};
           
            var roles = await _commandBus.SendAsync(new GetRolesQuery {ExcludedRoles = excludedRoles});
            var model = _mapper.Map<IEnumerable<RoleModel>>(roles);
            return model;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.POST, "SuperAdmin", "Admin")]

        public async Task<ActionResult<Role>> Post([FromBody] RoleModel model)
        {
            var role = new Role
            {
                Name = model.Name,
                NormalizedName = model.Name.ToUpper(),
            };

            await _commandBus.Send(new AddUpdateRoleCommand { Role = role });

            model = _mapper.Map<RoleModel>(role);

            return Created($"/api/roles/{model.Id}", model);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.PUT, "SuperAdmin", "Admin")]

        public async Task<ActionResult<Role>> Put([FromBody] RoleModel model)
        {
            var role = new Role
            {
                Id = model.Id,
                Name = model.Name,
                NormalizedName = model.Name.ToUpper(),
            };

            await _commandBus.Send(new AddUpdateRoleCommand { Role = role });

            model = _mapper.Map<RoleModel>(role);

            return Created($"/api/roles/{model.Id}", model);
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Membership, PermissionAction.DELETE, "SuperAdmin", "Admin")]
        public async Task<ActionResult> Delete(Guid id)
        {
           
            await _commandBus.Send(new DeleteRoleCommand { Id = id });
            return Ok();
        }

    }
}
