using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.ProductClasses.Commands;
using GHPCommerce.Application.Catalog.ProductClasses.DTOs;
using GHPCommerce.Application.Catalog.ProductClasses.Queries;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.ProductClasses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/catalog")]
    [ApiController]
    //[Authorize]
    public class CatalogController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public CatalogController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]
        public Task<PagingResult<ProductClassDto>> Get(string filer, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetProductClassesListQuery(filer, sort, page, pageSize));
        }
        [HttpPost("search")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup")]
        public Task<SyncPagedResult<ProductClassDto>> GetPaged(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetProductClassPagedQuery {GridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/catalog/getAll")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup", "Admin","OnlineCustomer")]

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IEnumerable<ProductClassDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllProductClassesQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/catalog/menu")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<IEnumerable<CatalogDto>> GetCatalogsForInventory()
        {
            return _commandBus.SendAsync(new GetCatalogsForCustomerQuery());
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup", "OnlineCustomer")]
        public Task<ProductClassDto> GetProductClassById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Product  class id should not be null or empty");
            return _commandBus.SendAsync(new GetProductClassByIdQuery {Id = id});

        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
       // [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Create, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Create([FromBody] ProductClassModel model)
        {
            var createProductClassCommand = _mapper.Map<CreateProductClassCommand>(model);
            var result = await _commandBus.SendAsync(createProductClassCommand);
            return ApiCustomResponse(result);
        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/catalog/{id:Guid}")]
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Update, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Update([FromRoute] Guid id, [FromBody] ProductClassModel model)
        {
            try
            {
                model.Id = id;
                var createProductClassCommand = _mapper.Map<UpdateProductClassCommand>(model);
                var result = await _commandBus.SendAsync(createProductClassCommand);
                return ApiCustomResponse(result);
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
        [Route("/api/catalog/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.DELETE, "TechnicalDirectorGroup")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteProductClassCommand {Id = id});
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
