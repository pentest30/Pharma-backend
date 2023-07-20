using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Sectors.Commands;
using GHPCommerce.Application.Tiers.Sectors.DTOs;
using GHPCommerce.Application.Tiers.Sectors.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.WebApi.Models.Organizations.Sectors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/sectors")]
    [ApiController]
    public class SectorsController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public SectorsController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.POST, "Admin")]
        public Task<PagingResult<SectorDto>> Get(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetSectorsListQuery(term, sort, page, pageSize));
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin", "Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/sectors/search")]
        [HttpPost]
        public async Task<SyncPagedResult<SectorDto>> GetPagedSectorList([FromBody] SyncDataGridQuery query)
        {
            return await _commandBus.SendAsync(new GetPagedSectorsList{ GridQuery = query });
        }
        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/sectors/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "Admin")]
        public Task<IEnumerable<SectorDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllSectorsQuery());
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.POST, "Admin")]
        public async Task<ActionResult> Add(SectorCustomerModel model)
        {

            var createCommand = _mapper.Map<CreateSectorCommand>(model);
            var result = await _commandBus.SendAsync(createCommand);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.PUT, "Admin")]
        [Route("/api/sectors/{id:Guid}")]
        public async Task<ActionResult> Put( SectorCustomerModel model)
        {
           
            var createCommand = _mapper.Map<UpdateSectorCommand>(model);
            var result = await _commandBus.SendAsync(createCommand);
            return ApiCustomResponse(result);
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/sectors/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "Admin")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();

            try
            {
                await _commandBus.Send(new DeleteSectorCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
}
