using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.TransactionTypes.Commands;
using GHPCommerce.Application.Catalog.TransactionTypes.DTOs;
using GHPCommerce.Application.Catalog.TransactionTypes.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/transactionType")]
    [ApiController]
    [Authorize]
    public class TransactionTypeController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public TransactionTypeController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("/api/transactionType/search")]
        public Task<SyncPagedResult<TransactionTypeDto>> Get(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedTransactionTypeQuery { DataGridQuery = query });
        }

        [HttpGet]
        [Consumes("application/json")]
        [Route("/api/transactionType/getAll")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup", "Admin")]

        public Task<IEnumerable<TransactionTypeDto>> GetAll()
        {
            return _commandBus.SendAsync(new GetAllTransactionTypeQuery());
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup", "Admin")]
        public async Task<ActionResult> Create(CreateTransactionTypeCommand model)
        {
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }

        [HttpPut]
        [Route("/api/transactionType/{Id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup", "Admin")]

        public async Task<ActionResult> Update(UpdateTransactionTypeCommand command)
        {

            try
            {
                var result = await _commandBus.SendAsync(command);
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
        [Route("/api/transactionType/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "TechnicalDirector", "Buyer", "BuyerGroup", "TechnicalDirectorGroup", "Admin")]
        public async Task<ActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest();
            try
            {
                await _commandBus.Send(new DeleteTransactionTypeCommand { Id = id });
                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }
    }
 }
