using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Hpcs;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
   using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Orders;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Infra.Filters;
using GHPCommerce.Infra.Web;
using GHPCommerce.Modules.Sales.Commands.DynamicsAx;
using GHPCommerce.Modules.Sales.Commands.Orders;
using GHPCommerce.Modules.Sales.Commands.SalesPerson.Commands;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.DTOs.Dynamicx;
using GHPCommerce.Modules.Sales.Queries;
using GHPCommerce.Modules.Sales.Queries.OnlineOrder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace GHPCommerce.Modules.Sales.Controllers
{
    [Route("api/orders")]
    [ApiController]
    //[Authorize]
    public class OrdersController : ApiController
    {
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly IFileProvider _fileProvider;
        private readonly MedIJKModel _model;

        public OrdersController(ICommandBus commandBus, ICurrentUser currentUser, IFileProvider fileProvider,MedIJKModel model)
        {
            _commandBus = commandBus;
            _currentUser = currentUser;
            _fileProvider = fileProvider;
            _model = model;
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "OnlineCustomer", "SalesPerson")]
        public Task<PagingResult<OrderDto>> Get(string term, string sort, int page, int pageSize)
        {
            Guid? customerId = default;
            if (User.IsInRole("OnlineCustomer"))
                customerId = _currentUser.UserId;
            return _commandBus.SendAsync(new GetOrdersListQuery(term, sort, page, pageSize)
            { CurrentCustomerId = customerId });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/suppliers/{supplierId:Guid}/orders")]
        public Task<PagingResult<OrderDto>> GetOrdersForWholesaler(string term, string sort, int page, int pageSize)
        {
            return _commandBus.SendAsync(new GetListOfOrdersOfB2BCustomersQuery(term, sort, page, pageSize));
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/suppliers/{supplierId:Guid}/orders/search")]
        public Task<SyncPagedResult<OrderDto>> GetOrdersForWholesaler(SyncDataGridQuery query , [FromHeader] bool? isPsy)
        {
            DateTime? start = default;
            DateTime? end = default;
            var qSTart = Request.Headers["start"];
            var qEnd = Request.Headers["end"];
            if (qSTart != "null"&& qSTart != "{}" &&  qSTart.ToString() != "")
                start = DateTime.Parse(qSTart);
            if (qEnd!="null" && qEnd != "{}" &&   qEnd.ToString() != "")
                end = DateTime.Parse(qEnd);
            return _commandBus.SendAsync(new GetPagedOrdersQuery {SyncDataGridQuery = query, Start = start, End = end, IsPsy = isPsy});
        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "SalesManager", "SalesPerson", "Admin","Supervisor")]
        [Route("/api/orders/{Id:Guid}")]
        public async Task<ActionResult> Update(Guid id, UpdateOrderByPharmacistCommand model)
        {
            if (id != model.Id) return BadRequest();
            var result = await _commandBus.SendAsync(model);
            //Notify his sales person
            return Ok(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "SalesManager", "SalesPerson","Supervisor","OnlineCustomer")]
        [Route("/api/suppliers/{supplierId:Guid}/orders/{Id:Guid}/send")]
        public async Task<ActionResult> CheckOut(SendOrderByPharmacistCommand model)
        {
            if (model.Id == Guid.Empty) return BadRequest();
            var result = await _commandBus.SendAsync(model);
            //Notify his sales person
            return Ok(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "SalesManager","Admin","Supervisor")]
        [Route("/api/suppliers/{supplierId:Guid}/orders/{id:Guid}/cancel")]
        public async Task<ActionResult> Cancel(Guid id, Guid supplierId)
        {
           var result = await _commandBus.SendAsync(new CancelOrderCommand {Id = id});
           return Ok(result);
        }
        [HttpDelete]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/suppliers/{supplierId:Guid}/orders/{id:Guid}")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.DELETE, "SalesManager", "SalesPerson","Supervisor")]
        public async Task<ActionResult> CancelOrder(Guid id, Guid supplierId)
        {
            if (id == Guid.Empty) return BadRequest();
            var result = await _commandBus.SendAsync(new CancelOrderByPharmacistCommand { Id = id,SupplierId = supplierId });
            return Ok(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/orders/{orderId:Guid}/items/")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "SalesManager","OnlineCustomer", "SalesPerson","Supervisor")]
        public async Task<ActionResult> 
            Item(Guid orderId, OrderItemCreateCommandV2 model)
        {
            if (orderId != model.OrderId) return BadRequest();
            var task = _commandBus.SendAsync(model);
            var result = await task.ConfigureAwait(false);
            return ApiCustomResponse(result);
         

        }
        #region OrderItem Endpoints ,Param:FEFO=True with MinExpiryDate condition
 
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/orders/{orderId:Guid}/items/")]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.Update, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult> UpdateItem(Guid orderId, OrderItemUpdateCommand model)
        {
            if (orderId != model.OrderId) return BadRequest();
            //To be fixed : var createCommand = _mapper.Map<OrderItemCreateCommand>(model);
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/orders/{orderId:Guid}/items/sales-person")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT,  "SalesPerson","SalesManager", "Supervisor")]
        public async Task<ActionResult> UpdateItem(Guid orderId, OrderItemUpdateCommandV2 model)
        {
            if (orderId != model.OrderId) return BadRequest();
            //To be fixed : var createCommand = _mapper.Map<OrderItemCreateCommand>(model);
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);

        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Route("/api/orders/{orderId:Guid}/deleteItem/")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.DELETE, "OnlineCustomer", "SalesPerson", "Admin","Supervisor")]
        public async Task<ActionResult> DeleteItem(Guid orderId, OrderItemDeleteCommand model)
        {
            if (orderId != model.OrderId) return BadRequest();

            //To be fixed : var createCommand = _mapper.Map<OrderItemCreateCommand>(model);
            var result = await _commandBus.SendAsync(model);
            return ApiCustomResponse(result);
        }
        #endregion
        #region OrderItem Endpoints ,Param:FEFO=False,
        /*
         * Batch Number has to be requested
         */
        #endregion

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/customer/{supplierId:Guid}/orders/pending")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "SalesManager", "SalesPerson","Admin","Supervisor")]
        public async Task<ActionResult<OrderDto>> GetPendingOrderSupplier([FromRoute] Guid supplierId)
        {
            if (supplierId == Guid.Empty)
                throw new InvalidOperationException("Supplier id should not be null or empty");
            var result = await _commandBus.SendAsync(new GetPendingOrderSupplierQuery { SupplierId = supplierId });
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/customer/{supplierId:Guid}/pending-orders/all")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "OnlineCustomer", "SalesPerson", "SalesManager", "Admin","Supervisor")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetAllPendingOrderSupplier()
        {
           
            var result = await _commandBus.SendAsync(new GetAllPendingOrdersForSalePersonQuery ());
            return Ok(result);
        }

        
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/salesperson/{orderId:Guid}/{customerId:Guid}/{salesPersonId:Guid}/orders/pending")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "SalesManager", "SalesPerson", "Admin","Supervisor")]
        public async Task<ActionResult<OrderDto>> GetSalesPersonPendingOrder([FromRoute] Guid orderId, Guid customerId, Guid salesPersonId)
        {
            if (orderId == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            var result = await _commandBus.SendAsync(new GetSalesPersonPendingOrderQuery { OrderId = orderId, CustomerId = customerId, SalesPersonId = salesPersonId});
            return Ok(result);
        }

        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/salesperson/{customerId:Guid}/orders/pending/v2")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "SalesManager", "SalesPerson", "Admin","Supervisor")]
        public async Task<ActionResult<OrderDto>> GetSalesPersonPendingOrderV2([FromRoute] Guid customerId)
        {
            if (customerId == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            var result = await _commandBus.SendAsync(new GetSalesPersonPendingOrderQueryV2 { CustomerId = customerId });
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/salesperson/{customerId:Guid}/orders/pending-orders")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET,  "SalesPerson","Supervisor","SalesManager")]
        public async Task<ActionResult<OrderDto>> GetSalesPersonPendingOrders([FromRoute] Guid customerId)
        {
            if ( customerId == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            var result = await _commandBus.SendAsync(new GetPendingOrdersForSalePersonQuery { CustomerId = customerId });
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/supervisor/orders/all-pending-orders")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET,  "SalesPerson","SalesManager", "Supervisor")]
        public async Task<ActionResult<OrderDto>> GetAllPendingOrders()
        {
            var result = await _commandBus.SendAsync(new GetPendingOrdersForSupervisors() );
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/orders/{id:Guid}/")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "SalesManager", "SalesPerson", "Admin","Supervisor","OnlineCustomer", "Controller")]
        public Task<OrderDtoV2> GetOrderByIdGroupedByProduct([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            return _commandBus.SendAsync(new GetOrderByIdQuery { Id = id });

        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/api/orders/{id:Guid}/v1")]
        [ResourceAuthorization(PermissionItem.Catalog, PermissionAction.GET, "SalesManager", "SalesPerson", "Admin","Supervisor")]
        public Task<OrderDtoV2> GetOrderById([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            return _commandBus.SendAsync(new GetOrderByIdV1Query { Id = id });

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{id:Guid}/changeState")]
        public async Task<ActionResult> ChangeState(ChangeOrderStateCommand command)
        {
            if (command.Id == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            var result =  await _commandBus.SendAsync(command);
            return Ok(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{id:Guid}/paid")]
        public async Task<ActionResult> PaidOrder(ChangePaymentStateCommand command)
        {
            if (command.Id == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            var result = await _commandBus.SendAsync(command);
            return Ok(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.DELETE, "SalesManager", "SalesPerson", "Supervisor")]
        [Route("/api/orders/{id:Guid}/remove-pending")]
        public async Task<ActionResult> RemovePendingOrder(OrderDto command)
        {
            if (command.Id == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            var result = await _commandBus.SendAsync(new CancelPendingOrderCommand {Order = command});
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "SalesManager", "SalesPerson", "Supervisor")]
        [Route("/api/cached-orders/pending-detail")]
        public async Task<ActionResult> GetPendingOrderDetail()
        {
             var result = await _commandBus.SendAsync(new GetDetailPendingOrdersQuery());
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "SalesManager", "SalesPerson", "Supervisor")]
        [Route("/api/orders/all-reserved-qnt")]
        public async Task<ActionResult> GetReservedQuantities()
        {
            var result = await _commandBus.SendAsync(new GetReservedQuantitiesQuery());
            return Ok(result);
        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{id:Guid}/extra-discount")]
        public async Task<ActionResult> ChangeExtraDiscount([FromRoute] Guid id, ChangeExtraDiscountCommand model)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            model.Id = id;
            var result = await _commandBus.SendAsync(model);
            return Ok(result);
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{id:Guid}/discounts/multi")]
        public async Task<ActionResult> ApplyDiscount([FromRoute] Guid id, UpdateOrderDiscountCommandV2 model)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            model.Id = id;
            var result = await _commandBus.SendAsync(model);
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{customerId:Guid}/order-items/{productCode}")]
        public async Task<IEnumerable<OrderHistoryDto>> ChangeExtraDiscount([FromRoute] Guid customerId, [FromRoute] string productCode)
        {
            return await _commandBus.SendAsync(new GetOrderHistoryByProductCodeQuery {CustomerId = customerId, ProductCode = productCode });
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "SalesManager", "SalesPerson" ,"Supervisor")]
        [Route("/api/orders/{customerId:Guid}/all")]
        public async Task<IEnumerable<OrderDto>> GetAllOrders([FromRoute] Guid customerId )
        {
            return await _commandBus.SendAsync(new GetValidOrdersForCustomerQuery { CustomerId = customerId});
        }

        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/preparationorder/{orderId:Guid}/generate/")]
        public async Task<ActionResult> GeneratePreparationOrder(Guid orderId)
        {
          
            var result =  await _commandBus.SendAsync(new GeneratePreparationOrderCommand { OrderId = orderId});
            return ApiCustomResponse(result);

        }
       [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{orderNumber:int}/created-ax/")]
        public async Task<ActionResult> AxOrderCreated(UpdateOrderCode model )
        {
          
            var result =  await _commandBus.SendAsync(new OrderAxCreatedCommand {OrderNumber = model.OrderNumber, CodeAx = model.CodeAx});
            return ApiCustomResponse(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{orderNumber:int}/{codeAx}/created-ax/")]
        public async Task<ActionResult> AxOrderCreated([FromRoute] int orderNumber,[FromRoute] string codeAx )
        {
          
            var result =  await _commandBus.SendAsync(new OrderAxCreatedCommand {OrderNumber = orderNumber, CodeAx =codeAx});
            return ApiCustomResponse(result);

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{orderNumber:int}/error-ax/")]
        public async Task<ActionResult> AxErrorOnCreatingOrder( OrderAxErrorCommand command)
        {
          
            var result =  await _commandBus.SendAsync(command);
            return ApiCustomResponse(result);

        }

        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{code}/update-status-ax/")]
        public async Task<ActionResult> UpdateOrderStatus([FromRoute] string code, UpdateOrderAxStatus order)
        {
            order.CodeAx = code;
            var result =  await _commandBus.SendAsync(order);
            return ApiCustomResponse(result);

        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/Ax2012/update-status-ax/")]
        public async Task<ActionResult> UpdateOrderStatus(UpdateOrderAxStatus order)
        {
            var result =  await _commandBus.SendAsync(order);
            return ApiCustomResponse(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{id:guid}/print/")]
        public async Task<IActionResult> PrintOrder([FromRoute] Guid id)
        {
            var path =  await _commandBus.SendAsync(new PrintOrderCommand {OrderId = id});
            return PhysicalFile(path, "application/pdf", "myfile.pdf");
                  
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin", "SalesManager", "SalesPerson", "PrintingAgent","Supervisor")]
        [Route("/api/orders/{customerId}/printingAgent/v1")]
        public async Task<SyncPagedResult<OrderDtoV4>> GetOrdersForPrintingAgent(SyncDataGridQuery query, [FromRoute] Guid customerId)
        {
            
            return await _commandBus.SendAsync(new GetOrdersByStatusQuery { DataGridQuery = query
                , Status =(int)Entities.OrderStatus.Prepared
                });
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "Admin", "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{code}/driver/ax")]
        public async Task<ActionResult> UpdateDriverName([FromRoute] string code, SyncDriverNameCommand command)
        {
            var result =  await _commandBus.SendAsync( command);
            return ApiCustomResponse(result);

        }

      
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/orders/{organizationId:Guid}/{customerCode}")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult<List<OrderTableModel>>> GetOrders(string customerCode, Guid organizationId)
        {
            if (string.IsNullOrEmpty(customerCode)|| organizationId==Guid.Empty) return BadRequest(null);
            var result = await _commandBus.SendAsync(new GetOrdersByOnlineCustomer() { 
            CustomerCode=customerCode,
            OrganizationId=organizationId 
            });
            if (result == null) return NotFound(result);
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/orders/{organizationId:Guid}/{customerCode}/{orderNumber}/{orderDate}")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult<List<OrderLineModel>>> GetOrderLines(string customerCode, Guid organizationId,string orderNumber,DateTime orderDate)
        {
            if (string.IsNullOrEmpty(customerCode) || organizationId == Guid.Empty) return BadRequest(null);
            var result = await _commandBus.SendAsync(new GetOrderDetailsByOnlineCustomer()
            {
                CustomerCode = customerCode,
                OrganizationId = organizationId,
                OrderNumber= orderNumber,OrderDate=orderDate
            });
            if (result == null) return NotFound(result);
            return Ok(result);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("/api/orders/lookup/{organizationId:Guid}/{customerCode}/{filter}")]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.GET, "OnlineCustomer", "SalesPerson")]
        public async Task<ActionResult<List<string>>> OrderLookup(string customerCode, Guid organizationId, string filter)
        {
            if (string.IsNullOrEmpty(customerCode) || organizationId == Guid.Empty) return BadRequest(null);
            var result = await _commandBus.SendAsync(new OrderLoopkupByOnlineCustomer()
            {
                CustomerCode = customerCode,
                OrganizationId = organizationId,
                Filter = filter
            });
            if (result == null) return NotFound(result);
            return Ok(result);
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin","OnlineCustomer")]
        [Route("/api/customers/{customerId:Guid}/orders/search")]
        public Task<SyncPagedResult<OrderDto>> GetOrdersForCustomer(SyncDataGridQuery query)
        {
            return _commandBus.SendAsync(new GetOrdersForCustomerQuery {SyncDataGridQuery = query});
        }
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST,"TechnicalDirector", "TechnicalDirectorGroup")]
        [Route("/api/orders/generate-op")]
        public async Task<ValidationResult> GenerateOps(GeneratePreparationOrderCommand command)
        {
            return await _commandBus.SendAsync(command);
        }
        [HttpGet]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ResourceAuthorization(PermissionItem.Sales, PermissionAction.POST, "Admin","OnlineCustomer")]
        [Route("/api/orders/today")]
        public Task<List<OrderByDateDto>> GetOrdersToday()
        {
            return _commandBus.SendAsync(new GetValidOrdersOfDayQuery( ));
        }
        [HttpPut]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ResourceAuthorization(PermissionItem.Sales, PermissionAction.PUT, "SalesManager", "SalesPerson","Supervisor")]
        [Route("/api/orders/{id:Guid}/special-order/{customerId:guid}")]
        public async Task<ActionResult> ChangeSpecialOrder([FromRoute] Guid id, [FromRoute] Guid customerId)
        {
            if (id == Guid.Empty)
                throw new InvalidOperationException("Order id should not be null or empty");
            ChangeSpecialOrderCommand model = new ChangeSpecialOrderCommand();
            model.Id = id;
            model.CustomerId = customerId;
            var result = await _commandBus.SendAsync(model);
            return Ok(result);
        }

    }

}