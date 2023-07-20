using System;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Customer.CreditNotes.DTOs; 
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries; 
using GHPCommerce.Infra.Web; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GHPCommerce.Modules.Sales.Queries.CreditNotes;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Modules.Sales.Commands.CreditNotes;


namespace GHPCommerce.Modules.Sales.Controllers
{
    [Route("api/credit-notes")]
    [ApiController]
    public class CreditNotesController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;

        public CreditNotesController(ICommandBus commandBus, ICurrentUser currentUser)
        {
            _commandBus = commandBus;
            _currentUser = currentUser;
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/credit-notes/search")]
        public Task<SyncPagedResult<CreditNoteDto>> GetCreditNotesForWholesaler(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedCreditNotesQuery { SyncDataGridQuery = query });
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/credit-notes/{creditNoteId:Guid}")]

        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.PUT, "Admin", "ClaimManager")]

        public async Task<ActionResult> UpdateCreditNote(Guid creditNoteId, [FromBody] UpdateCreditNoteCommand command)
        {
            if (command.Id != creditNoteId)
                return BadRequest();
            var result = await _commandBus.SendAsync(command);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/credit-notes/{creditNoteId:Guid}/validate")]

        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "Admin", "ClaimManager")]

        public async Task<ActionResult> ValidateCreditNote(Guid creditNoteId, [FromBody] ValidateCreditNoteCommand command)
        {
            if (command.Id != creditNoteId)
                return BadRequest();
            var result = await _commandBus.SendAsync(command);
            return ApiCustomResponse(result);
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/credit-notes/{creditNoteId:Guid}")]

        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.DELETE, "Admin", "ClaimManager")]

        public async Task<ActionResult> DeleteCreditNote(Guid creditNoteId, [FromBody] DeleteCreditNoteCommand command)
        {
            if (command.Id != creditNoteId)
                return BadRequest();
            var result = await _commandBus.SendAsync(command);
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "ClaimManager")]
        [Route("/api/credit-notes/print/{id:Guid}/")]
        public async Task<ActionResult> PrintSalesInvoice([FromRoute] Guid id)
        {
            var result = await _commandBus.SendAsync(new PrintCreditNoteCommand { Id = id });
            return Ok();

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "ClaimManager")]
        [Route("/api/credit-notes/{id:GUID}")]
        public async Task<CreditNoteDto> GetById(Guid id)
        {
            return await _commandBus.SendAsync(new GetCreditNoteByIdQuery { Id = id });
        }

    }
}