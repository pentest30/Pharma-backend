using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Packaging.Commands;
using GHPCommerce.Application.Catalog.Packaging.DTOs;
using GHPCommerce.Application.Catalog.Packaging.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.Packaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/packaging")]
    [ApiController]
    [Authorize]
    public class PackagingController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public PackagingController( IMapper mapper, ICommandBus commandBus)
        {
            _mapper = mapper;
            _commandBus = commandBus;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<PackagingDto>> Get(string term, string sort,int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetPackagingListQuery (term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/packaging/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]
        public Task<IEnumerable<PackagingDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllPackagingQuery());
        }
        [HttpGet]
        [Route("/api/packaging/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PackagingDto> GetPackagingById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Packaging id should not be null or empty");
            return _commandBus.SendAsync(new GetPackagingByIdQuery {Id = id});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.POST,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create(PackagingModel model)
        {
            var createProductClassCommand = _mapper.Map<CreatePackagingCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/packaging/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
         [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, PackagingModel model)
        {
            if (id != model.Id)
                return BadRequest();
            var updatePackagingCommand = _mapper.Map<UpdatePackagingCommand>(model);
            var result = await _commandBus.SendAsync(updatePackagingCommand);
            return ApiCustomResponse(result);
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<SyncPagedResult<PackagingDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPackagingPagedQuery() { GridQuery = query });
        }
        [HttpDelete]
        [Route("/api/packaging/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeletePackagingCommand {Id = id});
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
