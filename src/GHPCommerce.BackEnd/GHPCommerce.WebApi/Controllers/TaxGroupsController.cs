using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.TaxGroups.Commands;
using GHPCommerce.Application.Catalog.TaxGroups.DTOs;
using GHPCommerce.Application.Catalog.TaxGroups.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.TaxGroups;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/taxGroups")]
    [ApiController]
    //[Authorize]
    public class TaxGroupsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public TaxGroupsController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<TaxGroupDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetTaxGroupsListQuery (term, sort, page, pageSize));
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/taxGroups/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup", "Admin")]

        public Task<IEnumerable<TaxGroupDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllTaxesQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/taxGroups/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<TaxGroupDto> GetProductClassById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Tax group id should not be null or empty");
            return _commandBus.SendAsync(new GetTaxGroupsByIdQuery { Id = id });

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Create,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create(TaxGroupModel model)
        {
            var createProductClassCommand = _mapper.Map<CreateTaxGroupCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/taxGroups/{code}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
         //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Update,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update( TaxGroupModel model)
        {
           
            try
            {
                var createProductClassCommand = _mapper.Map<UpdateTaxGroupCommand>(model);
                var result = await _commandBus.SendAsync(createProductClassCommand);
                return ApiCustomResponse(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<SyncPagedResult<TaxGroupDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetTaxGroupsPagedQuery() { GridQuery = query });
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/taxGroups/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteTaxGroupCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
