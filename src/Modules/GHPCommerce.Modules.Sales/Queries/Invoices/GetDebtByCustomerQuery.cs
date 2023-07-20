using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Core.Shared.Services.ExternalServices;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Modules.Sales.DTOs.Invoices;
using RestSharp;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public class GetDebtByCustomerQuery : ICommand<IEnumerable<DebtDto>>
    {
        public Guid CustomerId{ get; set; }
    }

    public class GePagedDebtByCustomerQuery : ICommand<SyncPagedResult<DebtDetailDto>>
    {
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
     public class GePagedDebtByCustomerQueryHandler : ICommandHandler<GePagedDebtByCustomerQuery,SyncPagedResult<DebtDetailDto> >
     {
         private readonly DeptServiceConfig _config;
         private readonly ICallApiService _callApiService;
         private readonly ICommandBus _commandBus;

         public GePagedDebtByCustomerQueryHandler(DeptServiceConfig config, ICallApiService callApiService, ICommandBus commandBus)
         {
             _config = config;
             _callApiService = callApiService;
             _commandBus = commandBus;
         }

         public async Task<SyncPagedResult<DebtDetailDto>> Handle(GePagedDebtByCustomerQuery request, CancellationToken cancellationToken)
         {
             var url = _config.Url +  $"all/{request.SyncDataGridQuery.Skip}/{request.SyncDataGridQuery.Take}" ;
             if (request.SyncDataGridQuery.Where != null)
             {
                 foreach (var wherePredicate in request.SyncDataGridQuery.Where[0].Predicates)
                 {
                     if (wherePredicate.Value == null)
                         continue;
                     if (wherePredicate.Field == "customerName")
                     {
                         url += $"/{wherePredicate.Value}";
                     }
                     
                 }
             }
           
             var result = await _callApiService.ExecuteRestRequestAsync<DebtDetailResultDto>(url, null, Method.GET);
             return new SyncPagedResult<DebtDetailDto> {Result = result.Data.Data, Count = result.Data.TotalItems};
         }
     }

     public class GetDetailsDebtByCustomerQuery : ICommand<SyncPagedResult<DebtDetailDto>>
    {
        public Guid CustomerId{ get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
     public class GetDetailsDebtByCustomerQueryV1 : ICommand<SyncPagedResult<DebtDetailDto>>
     {
         public string CustomerCode{ get; set; }
         public SyncDataGridQuery SyncDataGridQuery { get; set; }
     }
    public  class GetDebtByCustomerQueryHandler : ICommandHandler<GetDebtByCustomerQuery, IEnumerable<DebtDto>>
    {
        private readonly DeptServiceConfig _config;
        private readonly ICallApiService _callApiService;
        private readonly ICommandBus _commandBus;

        public GetDebtByCustomerQueryHandler(DeptServiceConfig config, ICallApiService callApiService, 
            ICommandBus commandBus)
        {
            _config = config;
            _callApiService = callApiService;
            _commandBus = commandBus;
        }
        public async Task<IEnumerable<DebtDto>> Handle(GetDebtByCustomerQuery request, CancellationToken cancellationToken)
        {
            var customer = await _commandBus.SendAsync(new GetCustomerByIdQuery { Id = request.CustomerId },cancellationToken);
            var url = _config.Url + _config.Organization + "/" + customer.Code;
            var result = await _callApiService.ExecuteRestRequestAsync<IEnumerable<DebtDto>>(url, null, Method.GET);
            return result.Data;
        }
    }
     public  class GetDetailsDebtByCustomerQueryHandler : ICommandHandler<GetDetailsDebtByCustomerQuery, SyncPagedResult<DebtDetailDto>>
     {
         private readonly DeptServiceConfig _config;
         private readonly ICallApiService _callApiService;
         private readonly ICommandBus _commandBus;

         public GetDetailsDebtByCustomerQueryHandler(DeptServiceConfig config, ICallApiService callApiService, 
             ICommandBus commandBus)
         {
             _config = config;
             _callApiService = callApiService;
             _commandBus = commandBus;
         }
         public async Task<SyncPagedResult<DebtDetailDto>> Handle(GetDetailsDebtByCustomerQuery request, CancellationToken cancellationToken)
         {
             var customer = await _commandBus.SendAsync(new GetCustomerByIdQuery { Id = request.CustomerId },cancellationToken);
             var url = _config.Url + _config.Organization + "/" + customer.Code + "/" + request.Month+"/"+ request.Year + "/"  + request.SyncDataGridQuery.Skip + "/" + request.SyncDataGridQuery.Take;
             var result = await _callApiService.ExecuteRestRequestAsync<DebtDetailResultDto>(url, null, Method.GET);
             return new SyncPagedResult<DebtDetailDto> { Result = result.Data.Data, Count = result.Data.TotalItems };
         }
     }
     public class  GetDetailsDebtByCustomerQueryV1Handler : ICommandHandler<GetDetailsDebtByCustomerQueryV1, SyncPagedResult<DebtDetailDto>>
     {
         private readonly DeptServiceConfig _config;
         private readonly ICallApiService _callApiService;
         private readonly ICommandBus _commandBus;

         public GetDetailsDebtByCustomerQueryV1Handler(DeptServiceConfig config, ICallApiService callApiService, 
             ICommandBus commandBus)
         {
             _config = config;
             _callApiService = callApiService;
             _commandBus = commandBus;
         }
         public async Task<SyncPagedResult<DebtDetailDto>> Handle(GetDetailsDebtByCustomerQueryV1 request, CancellationToken cancellationToken)
         {
             var url = _config.Url +  "all/byCustomer/" + request.CustomerCode + "/"  + request.SyncDataGridQuery.Skip + "/" + request.SyncDataGridQuery.Take;
             var result = await _callApiService.ExecuteRestRequestAsync<DebtDetailResultDto>(url, null, Method.GET);
             return new SyncPagedResult<DebtDetailDto> { Result = result.Data.Data, Count = result.Data.TotalItems };

         }
     }
}