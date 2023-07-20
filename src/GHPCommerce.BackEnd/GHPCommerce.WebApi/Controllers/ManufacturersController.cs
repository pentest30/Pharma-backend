using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Manufacturers.Commands;
using GHPCommerce.Application.Catalog.Manufacturers.DTOs;
using GHPCommerce.Application.Catalog.Manufacturers.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.Manufacturers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/manufacturers")]
    [ApiController]
    public class ManufacturersController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;

        public ManufacturersController(IMapper mapper, ICommandBus commandBus)
        {
            _mapper = mapper;
            _commandBus = commandBus;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<PagingResult<ManufacturerDto>> Get(string term, string sort ,int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetManufacturersListQuery (term, sort,page, pageSize));
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<SyncPagedResult<ManufacturerDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetManufacturersPagedQuery()  {GridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/manufacturers/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public Task<IEnumerable<ManufacturerDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllManufacturersQuery());
        }
        [HttpGet]
        [Route("/api/manufacturers/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector","Buyer", "BuyerGroup", "TechnicalDirectorGroup")]

        public async Task<ManufacturerDto> GetManufacturerById([FromRoute] Guid id)
        {
            if (id == default)
                throw new InvalidOperationException("Manufacturer id should not be null or empty");

            var manufacturerById = await _commandBus.SendAsync(new GetManufacturerByIdQuery { Id = id });
            return manufacturerById;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
      //  [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Create,  "TechnicalDirectorGroup")]

        public async Task<ActionResult> Create([FromBody] ManufacturerModel model)
        {
            var createProductClassCommand = _mapper.Map<CreateManufacturerCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/manufacturers/{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Update([FromRoute] Guid id, ManufacturerModel model)
        {
            
            var updateManufacturerCommand = _mapper.Map<UpdateManufacturerCommand>(model);
            var result = await _commandBus.SendAsync(updateManufacturerCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Route("/api/manufacturers/{code}/ax")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Update, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Put([FromRoute] string code, ManufacturerModel model)
        {
            if (code != model.Code)
                return BadRequest("Bad Manufacturer code");
          
            var updateManufacturerCommand = _mapper.Map<UpdateManufacturerByCodeCommand>(model);
            var result = await _commandBus.SendAsync(updateManufacturerCommand);
            return ApiCustomResponse(result);
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/manufacturers/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteManufacturerCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/manufacturers/{code}/ax")]
       // [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Delete, "TechnicalDirectorGroup")]

        public async Task<ActionResult> Delete([FromRoute] string code)
        {
            if (code == string.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteManufacturerByCodeCommand { Code = code});
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
