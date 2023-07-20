using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Core.Shared.Contracts.Quota;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Quota.Commands;
using GHPCommerce.Modules.Quota.Commands.QuotaRequests;
using GHPCommerce.Modules.Quota.DTOs;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Models;
using GHPCommerce.Modules.Quota.Queries;
using GHPCommerce.Modules.Quota.Queries.QuotaRequest;
using GHPCommerce.Modules.Quota.Queries.ReceivedQuota;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.Modules.Quota.Controllers
{
    [Route("api/quotas")]
    [ApiController]
    public class QuotasController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;

        public QuotasController(ICommandBus commandBus, ICurrentOrganization currentOrganization)
        {
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer","SalesPerson")]
        [Route("/api/quotas/search")]
        public Task<SyncPagedResult<QuotaDto>> Get(SyncDataGridQuery query)
        {
            var ids = Request.Headers["customerIds"];
            if (string.IsNullOrEmpty(ids))
                return _commandBus.SendAsync(new GetPagedQuotasQuery { DataGridQuery = query });
            var customerIds = ids.ToString().Split(',').Select(Guid.Parse).ToList();
            return _commandBus.SendAsync(new GetPagedQuotasQuery {DataGridQuery = query});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "BuyerGroup", "Buyer","SalesPerson")]
        [Route("/api/quotas-init/search")]
        public Task<SyncPagedResult<QuotaInitState>> GetInitQuota(SyncDataGridQuery query, [FromHeader]string start ,  [FromHeader] Guid salesPersonId, [FromHeader]Guid pId)
        {
            return _commandBus.SendAsync(new GetPagedQuotaInitStateQuery {DataGridQuery = query, 
                DateTime = DateTime.Parse(start),
                SalesPersonId = salesPersonId,ProductId = pId});
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson","Admin","Buyer","BuyerGroup")]
        [Route("/api/quotas/request/search")]
        public Task<SyncPagedResult<QuotaRequestDto>> GetRequestedQuota(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedRequestedQuotasQuery {DataGridQuery = query});
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson","Admin", "Buyer", "BuyerGroup")]
        [Route("/api/quotas/request/total")]
        public Task<int> GetRequestedQuotaCount()
        {
            return _commandBus.SendAsync(new GetTotalRequestQuotaQuery());
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson", "Buyer", "BuyerGroup", "Admin")]
        [Route("/api/quotas/customers")]
        public async Task<ActionResult> GetCustomersQuota()
        {
            var s =  await _commandBus.SendAsync(new PrintCustomerQuotaCommand());
            string excelName = $"quota-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            return PhysicalFile(s, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "SalesPerson","Supervisor","SalesManager", "Admin", "Buyer", "BuyerGroup")]
        [Route("/api/quotas/{productId:guid}/sales-person")]
        public Task<IEnumerable<QuotaDto>> GetQuotasByProduct([FromRoute]Guid productId )
        {
            return _commandBus.SendAsync(new GetQuotasByProductQuery {ProductId = productId});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Supervisor", "SalesPerson" ,"Admin", "Buyer", "BuyerGroup")]
        [Route("/api/quotas/received/search")]
        public Task<SyncPagedResult<ReceivedQuotaDto>> GetReceivedQuota(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetPagedReceivedQuotasQuery {DataGridQuery = query});
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Buyer", "BuyerGroup")]

        public Task<QuotaDtoV1> Get(Guid productId, string date)
        {
            DateTime? start = DateTime.Parse(date);
            return _commandBus.SendAsync(new GetQuotaByProductIdQuery {ProductId = productId, Date = start.Value});
        }

        [HttpGet("{customerId:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "SalesPerson","SalesManager","Supervisor", "Admin", "Buyer", "BuyerGroup")]

        public Task<IEnumerable<QuotaDtoV1>> Get([FromRoute] Guid customerId)
        {
            return _commandBus.SendAsync(new GetQuotaByCustomerQuery());
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.POST, "Buyer", "BuyerGroup")]
        public async Task<ActionResult> Create(CreateQuotasModel command)
        {
            var validations = new List<ValidationResult>();
           // var s = new SemaphoreSlim(1, 1);
           var stp = new Stopwatch();
           stp.Start();
           var organizationId = await _currentOrganization.GetCurrentOrganizationIdAsync().ConfigureAwait(false);
           var commandInitList = new CreateQuotaInitStateCommands();
           foreach (var createQuotaCommand in command.Quotas.ShallowClone())
           {
               commandInitList.CreateQuotaInitStateCommandList.Add( new CreateQuotaInitStateCommand(createQuotaCommand, organizationId));
           }

           Task.Run(async () => await _commandBus.SendAsync(commandInitList));
           foreach (var createQuotaCommandGroup in command.Quotas.GroupBy(x=>x.SalesPersonId))
           {
               var cmd = createQuotaCommandGroup.FirstOrDefault().ShallowClone();
               if (cmd != null)
               {
                   cmd.InitialQuantity = createQuotaCommandGroup.Sum(x => x.InitialQuantity);
                   var result = await _commandBus.SendAsync(cmd).ConfigureAwait(false);
                   if (result != default && result.Errors.Any()) validations.Add(result);

               }
           }

           stp.Stop();
           Console.WriteLine(stp.ElapsedMilliseconds);
           if (!validations.Any())
            {
                if (command.RequestId != Guid.Empty)
                    await _commandBus.Send(new ValidateQuotaRequestCommandV3 { Id = command.RequestId , Quotas= command.Quotas});
                return Ok(validations);
            }
            return BadRequest(validations);
        }
       

        [HttpPut("{id:Guid}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.POST, "Buyer", "BuyerGroup")]

        public async Task<ActionResult> Put([FromRoute] Guid id, ReleaseQuotaCommand command)
        {
            var result = await _commandBus.SendAsync(command).ConfigureAwait(false);
            return ApiCustomResponse(result);
        }

        [HttpPost("request")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.POST, "SalesPerson", "Supervisor")]
        public async Task<ActionResult> Post(CreateQuotaRequestCommand command)
        {
            var result = await _commandBus.SendAsync(command).ConfigureAwait(false);
            return ApiCustomResponse(result);
        }

        [HttpPut]
        [Route("/api/quotas/validate/{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.POST, "SalesPerson" , "Supervisor", "Buyer")]
        public async Task<ActionResult> Validate([FromRoute] string id)
        {
            var result = await _commandBus.SendAsync(new ValidateQuotaRequestCommand {Id = Guid.Parse(id)}).ConfigureAwait(false);
            return ApiCustomResponse(result);
        }
        [HttpPut("validate/{id:guid}/customers")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.POST, "SalesPerson")]
        public async Task<ActionResult> ValidateByCustomer([FromRoute] Guid id, ValidateQuotaRequestCommandV2 command)
        {
            var result = await _commandBus.SendAsync(command).ConfigureAwait(false);
            return ApiCustomResponse(result);
        }
        [HttpPut("{id:guid}/reject")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.POST, "SalesPerson", "Supervisor", "Buyer", "BuyerGroup")]
        public async Task<ActionResult> RejectQuotaRequest([FromRoute] Guid id)
        {
           await _commandBus.Send(new RejectQuotaRequestCommand {Id = id}).ConfigureAwait(false);
            return Ok();
        }
        [HttpPost("{productId:Guid}/{quantity:int}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.POST, "SalesPerson", "Admin", "Buyer", "BuyerGroup")]

        public async Task<SyncPagedResult<QuotaDto>> Put([FromRoute] Guid productId, [FromRoute] int quantity, SyncDataGridQuery query)
        {
            var result = await _commandBus.SendAsync(new GetPagedQuotasByProductQuery
                {ProductId = productId, Quantity = quantity, DataGridQuery = query}).ConfigureAwait(false);
            return result;
        }
        [HttpGet("{productId:Guid}/sales-person/{salesPersonId:Guid}/details")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.POST, "SalesPerson","Buyer", "BuyerGroup", "Supervisor", "Admin")]

        public async Task<IEnumerable<QuotaDto>> GetList([FromRoute] Guid productId,[FromRoute] Guid salesPersonId)
        {
            var result = await _commandBus.SendAsync(new GetDetailedQuotaQuery()
                {ProductId = productId, SalesPersonId = salesPersonId }).ConfigureAwait(false);
            return result;
        }

        [HttpPut("update/{id:guid}/salesperson")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.PUT, "Supervisor")]
        public async Task<ActionResult> UpdateQuotaSalesPerson([FromRoute] Guid id, UpdateQuotaCommand command)
        {
            var result = await _commandBus.SendAsync(command).ConfigureAwait(false);
            return ApiCustomResponse(result);
        }
        [HttpPut("customers/release")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Quota, PermissionAction.PUT, "Supervisor")]
        public async Task<ActionResult> ReleaseQuotaByCustomer(ReleaseQuotaByCustomerCommand command)
        {
            var result = await _commandBus.SendAsync(command).ConfigureAwait(false);
            return ApiCustomResponse(result);
        }
    }
}