using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentScheduler;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.Commands.Orders;
using GHPCommerce.Modules.Sales.DTOs;
using RestSharp;

namespace GHPCommerce.Modules.Sales.Jobs
{
    public class DeletePendingOrdersJob : IJob
    {
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;
        public DeletePendingOrdersJob(ICommandBus commandBus, IMapper mapper)
        {
            _commandBus = commandBus;
            _mapper = mapper;
        }
        private static async Task<IRestResponse<T>> ExecuteRestRequestAsync<T>(string source, object body, Method method, string token = null)
        {
            var url = $"http://localhost:8080/{source}";
            RestClient restClient = new RestClient(url);
            RestRequest request = new RestRequest();
            request.AddHeader("Accept", "application/json; charset=utf-8 ");
            if (!string.IsNullOrEmpty(token))
            {
                request.AddHeader("Authorization", $"Bearer {token}");
            }

            if (method!= Method.GET)
            {
                request.RequestFormat = DataFormat.Json;
                request.Method = method;
                request.AddJsonBody(body);
            }
            var rsp = await restClient.ExecuteAsync<T>(request);
            return rsp;
        }
      
        public void Execute()
        {
            var body = new { userName = "userApi", password = "@000111" };
            var restResponse =  ExecuteRestRequestAsync<AccessToken>("api/users/signin", body, Method.POST).GetAwaiter().GetResult();
            if(!restResponse.IsSuccessful) return;
            var pendingOrders =  ExecuteRestRequestAsync<List<OrderDto>>("api/supervisor/orders/all-pending-orders",  null, Method.GET, restResponse.Data.Token).GetAwaiter().GetResult();
            foreach (var pendingOrder in pendingOrders.Data)
            {
                if (pendingOrder.OrderDate != null && (DateTime.Now.Date -  pendingOrder.OrderDate.Value.Date).Days >=0 && !pendingOrder.Psychotropic )
                {
                    var problemHappened = false;
                    foreach (var pendingOrderOrderItem in pendingOrder.OrderItems)
                    {
                        try
                        {
                            var deleteCommand = _mapper.Map<OrderItemUpdateCommandV2>(pendingOrderOrderItem);
                            deleteCommand.Quantity *= -1;
                            deleteCommand.OrderId = pendingOrder.Id;
                            if (pendingOrder.CustomerId != null)
                                deleteCommand.CustomerId = pendingOrder.CustomerId.Value;
                            if (pendingOrder.SupplierId != null)
                                deleteCommand.SupplierOrganizationId = pendingOrder.SupplierId.Value;
                            deleteCommand.CreatedByUserId = pendingOrder.CreatedByUserId;
                            var r = ExecuteRestRequestAsync<ValidationResult>($"api/orders/{pendingOrder.Id}/items/sales-person", deleteCommand, Method.PUT, restResponse.Data.Token).GetAwaiter().GetResult();
                            if (r.Data is { IsValid: false })
                            {
                                problemHappened = true;
                                break;
                            }


                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            //throw;
                        }       
                    }

                    if (!problemHappened)
                    {
                        foreach (var pendingOrderOrderItem in pendingOrder.OrderItems)
                        {
                            pendingOrderOrderItem.Quantity *= -1;
                        }
                        ExecuteRestRequestAsync<ValidationResult>($"api/orders/{pendingOrder.Id}/remove-pending/", pendingOrder, Method.PUT, restResponse.Data.Token).GetAwaiter().GetResult();
                    }
                        

                }
            }
            
            
        }
    }
    internal class AccessToken
    {
        public string Token { get; set; }
        public int Expiry { get; set; }
        public DateTime Date { get; set; }
    
    }
}