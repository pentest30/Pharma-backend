using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Tiers.Customers.Commands;
using GHPCommerce.Application.Tiers.Customers.DTOs;
using GHPCommerce.Application.Tiers.Customers.Queries;
using GHPCommerce.Application.Tiers.Guests.Commands;
using GHPCommerce.Application.Tiers.Organizations.DTOs;
using GHPCommerce.Application.Tiers.Suppliers.Commands;
using GHPCommerce.Application.Tiers.Suppliers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Modules.Sales.Models;
using GHPCommerce.WebApi.Models.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GHPCommerce.WebApi.Controllers
{
    [Route("api/customers")]
    [ApiController]
    [Authorize]
    public class CustomersController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;

        public CustomersController(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.POST,  "Admin")]
        public async Task<ActionResult<SupplierCustomer>> Assign([FromBody] AssignCustomerToSupplierModel model)
        {
            return ApiCustomResponse(await _commandBus.SendAsync(_mapper.Map<CreateSupplierCommand>(model)));
        }

        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET,  "Admin")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/potentials")]
        [HttpGet]
        public async Task<IEnumerable<OrganizationDto>> PotentialCustomersBySupplier()
        {
            return await _commandBus.SendAsync(new GetPotentialCustomersQuery());
        }

        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET,  "Admin", "")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<PagingResult<SupplierCustomerDto>> Get(string term, string sort, int page, int pageSize)
        {
            return await _commandBus.SendAsync(new GetCustomersQuery(term, sort, page, pageSize));
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/customers/{organizationId:Guid}/supplierCustomers/{id:Guid}")] 
        //[ResourceAuthorization(PermissionItem.Catalog, PermissionAction.Read, "BuyerGroup", "TechnicalDirector", "TechnicalDirectorGroup")]
        public Task<SupplierCustomerDto> GetSupplierCustomersById([FromRoute] Guid organizationId,[FromRoute] Guid id)
        {
            if (organizationId == Guid.Empty)
                throw new InvalidOperationException("client id should not be null or empty");
            if (id == Guid.Empty)
                throw new InvalidOperationException("organization id should not be null or empty");
            return _commandBus.SendAsync(new GetSupplierCustomerByIdQuery {Id = id, CustomerOrganizationId = organizationId});
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/customers/{organizationId:Guid}/supplierCustomers/{id:Guid}/change-status")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.PUT, "Admin")]
        public async Task<ActionResult> ChangeStatus([FromRoute] Guid organizationId, [FromRoute] Guid id)
        {
            if (organizationId == Guid.Empty)
                throw new InvalidOperationException("client id should not be null or empty");
            if (id == Guid.Empty)
                throw new InvalidOperationException("organization id should not be null or empty");
            await _commandBus.Send(new ChangeCustomerStatusCommand {Id = id, OrganizationId = organizationId});
            return Ok();
        }

        [HttpPut]
        [Consumes("application/json")]
        [Route("/api/customers/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.PUT, "Admin")]
       
        public async Task<IActionResult> Put( [FromRoute] Guid id, AssignCustomerToSupplierModel model)
        {
            
            if (id == Guid.Empty)
                throw new InvalidOperationException("client id should not be null or empty");
            var updateCommand = _mapper.Map<UpdateSupplierCustomerCommand>(model);
            updateCommand.Id = id;
            return ApiCustomResponse(await _commandBus.SendAsync(updateCommand));
        }

        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/salesperson")]
        [HttpGet]
        public async Task<IEnumerable<CustomerDto>> CustomerBySalesPerson( )
        {
          return await _commandBus.SendAsync(new GetCustomerBySalesPerson());
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Supervisor")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/{salesPersonId:Guid}/salesperson")]
        [HttpGet]
        public async Task<IEnumerable<CustomerDtoV1>> CustomerBySalesPersonId([FromRoute]Guid salesPersonId)
        {
            return await _commandBus.SendAsync(new GetCustomerBySalesPersonId {SalesPersonId = salesPersonId});
        }

        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SuperAdmin", "Admin", "Supervisor",
            "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/supervisor/search")]
        [HttpPost]
        public async Task<SyncPagedResult<CustomerDtoV1>> CustomersForSupervisor(SyncDataGridQuery query,[FromHeader] Guid? salesPersonId)
        {
           return await _commandBus.SendAsync(new GetPagedCustomersForSupervisor{ GridQuery = query, SalesPersonId = salesPersonId });
        }

        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin","Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/salesperson/post")]
        [HttpPost]
        public async Task<SyncPagedResult<CustomerDto>> CustomersBySalesPerson([FromBody]SyncDataGridQuery query)
        {
          return await _commandBus.SendAsync(new PagedCustomersBySalesPerson {  GridQuery = query });
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin","Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/salesperson/search")]
        [HttpGet]
        public async Task<IEnumerable<CustomerDto>> SearchCustomersBySalesPerson(string term)
        {
            return await _commandBus.SendAsync(new SearchCustomerQuery { Term = term});
        }

        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson","Admin", "SuperAdmin", "Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/salesperson/dash/search")]
        [HttpPost]
        public async Task<SyncPagedResult<CustomerDtoV2>> CustomerBySalesPersonDash(SyncDataGridQuery query)
        {
            return await _commandBus.SendAsync(new GetCustomerBySalesPersonDash { DataGridQuery = query });
        }

        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin", "SuperAdmin", "Supervisor", "SalesManager", "Buyer", "BuyerGroup")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/all")]
        [HttpGet]
        public async Task<IEnumerable<CustomerDto>> Customers()
        {
            return await _commandBus.SendAsync(new GetAllCustomersQuery());
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin", "SuperAdmin", "Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/salesperson/dash")]
        [HttpGet]
        public async Task<IEnumerable<CustomerDtoV2>> CustomerBySalesPersonIdDash()
        {
            return await _commandBus.SendAsync(new GetCustomersBySalesPersonIdDashQuery());
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin","Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/{id:Guid}/salesperson/get")]
        [HttpGet]
        public async Task<CustomerDto> CustomerBySalesPerson([FromRoute] Guid id)
        {
            return await _commandBus.SendAsync(new GetCustomerForSalesPersonById { CustomerId = id });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/customer/{id:Guid}/")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "OnlineCustomer", "SalesPerson", "Admin")]
        public async Task<CustomerDtoV1> GetCustomerById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Customer id should not be null or empty");
            return await _commandBus.SendAsync(new GetCustomerByIdQuery { Id = id });

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/customer/organization/{id:Guid}/")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "OnlineCustomer", "SalesPerson", "Admin")]
        public async Task<CustomerDtoV1> GetCustomerByOrganizationId([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Customer id should not be null or empty");
            return await _commandBus.SendAsync(new GetCustomerByOrganizationIdQuery { OrganizationId = id });

        }
        [HttpPost]
        [Route("/api/customer/guest")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitCommand(GuestPickupModel model)
        {
            var createCommand = _mapper.Map<CreateGuestPickupCommand>(model);
            var result = await _commandBus.SendAsync(createCommand, CancellationToken.None);
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Route("/api/customers/guest-ship")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitShippedCommand(GuestShipModel model)
        {
            var createCommand = _mapper.Map<CreateGuestShipCommand>(model);
            var result = await _commandBus.SendAsync(createCommand, CancellationToken.None);
            return ApiCustomResponse(result);
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SuperAdmin","Admin","Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/updateActualSalesPerson/")]
        [HttpPut]
        public async Task<IActionResult> UpdateActualSalesPerson(UpdateActualSalesPersonCommand command)
        {
            var result = await _commandBus.SendAsync(command, CancellationToken.None);
            return ApiCustomResponse(result);
            // return await _commandBus.SendAsync(new GetPagedCustomersForSupervisor {GridQuery = query});
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SuperAdmin","Admin","Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/cancelActualSalesPerson/")]
        [HttpPut]
        public async Task<IActionResult> CancelActualSalesPerson(RemoveActualSalesPersonCommand command)
        {
            var result = await _commandBus.SendAsync(command, CancellationToken.None);
            return ApiCustomResponse(result);
            // return await _commandBus.SendAsync(new GetPagedCustomersForSupervisor {GridQuery = query});
        }

        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin", "SuperAdmin", "Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/salesperson/today-dash")]
        [HttpGet]
        public async Task<DashboardDto> DailyBySalesPersonIdGlobalDash()
        {
            return await _commandBus.SendAsync(new GetDailyDashboardBySalesPersonIdDashQuery());
        }
        [ResourceAuthorization(PermissionItem.Tiers, PermissionAction.GET, "SalesPerson", "Admin", "SuperAdmin", "Supervisor", "SalesManager")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/customers/salesperson/monthly-dash")]
        [HttpGet]
        public async Task<DashboardDto> MonthlyBySalesPersonIdGlobalDash()
        {
            return await _commandBus.SendAsync(new GetMonthlyDashboardBySalesPersonIdDashQuery());
        }
    }
}

