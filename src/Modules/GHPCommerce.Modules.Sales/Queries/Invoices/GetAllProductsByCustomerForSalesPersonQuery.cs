using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Dapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs.Invoices;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Persistence;
using Microsoft.Data.SqlClient;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public  class  GetAllInvoiceProductsForSalesPersonQuery : ICommand<SyncPagedResult<SalesLogByProductDto>>,IGetPagedProductsByCustomerForSalesPersonQuery
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public Guid? CustomerId { get; set; }
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }

    public class GetAllProductsByCustomerForSalesPersonQuery :  ICommand<SyncPagedResult<SalesLogByProductDto>>,IGetPagedProductsByCustomerForSalesPersonQuery
    {
        public DateTime? Start { get; set; }
        public DateTime?  End { get; set; }
        public Guid? CustomerId { get; set; }
        public SyncDataGridQuery SyncDataGridQuery { get; set; }
    }
    public class GetAllProductsByCustomerForSalesPersonQueryV1 :ICommand<IEnumerable<SalesLogByProductDto>>,IGetAllProductsByCustomerForSalesPersonQuery
    {
        public DateTime? Start { get; set; }
        public DateTime?  End { get; set; }
        public Guid? CustomerId { get; set; }
    }
    public  class  GetAllInvoiceProductsForSalesPersonQueryHandler : ICommandHandler<GetAllInvoiceProductsForSalesPersonQuery, SyncPagedResult<SalesLogByProductDto>>
    {
        private readonly ConnectionStrings _connectionStrings;

        public GetAllInvoiceProductsForSalesPersonQueryHandler(ConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }
        public async Task<SyncPagedResult<SalesLogByProductDto>> Handle(GetAllInvoiceProductsForSalesPersonQuery request, CancellationToken cancellationToken)
        {
            var pagedQueryBuilder = new PagedQueryBuilder();
            var sqlCmd = pagedQueryBuilder.BuildInvoiceByProductsOfCustomerQuery(request);
            var sqlCmdTotal = pagedQueryBuilder.BuildTotalRecords(request);
            using (var cnn = new SqlConnection(_connectionStrings.ConnectionString))
            {
                var result = await cnn.QueryAsync<SalesLogByProductDto>(sqlCmd);

                var total = await cnn.QueryFirstOrDefaultAsync<Int32>(sqlCmdTotal);
                return new SyncPagedResult<SalesLogByProductDto> { Count = total, Result = result };
            }
        }
    }

    public  class GetAllProductsByCustomerForSalesPersonQueryV1Handler : ICommandHandler< GetAllProductsByCustomerForSalesPersonQueryV1 ,IEnumerable<SalesLogByProductDto>  >
    {
        private readonly ConnectionStrings _connectionStrings;

        public GetAllProductsByCustomerForSalesPersonQueryV1Handler(ConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }
        public async Task<IEnumerable<SalesLogByProductDto>> Handle(GetAllProductsByCustomerForSalesPersonQueryV1 request, CancellationToken cancellationToken)
        {
            var allQueryBuilder = new GetAllQueryBuilder();
            var cmd = allQueryBuilder.BuildInvoiceByProductsOfCustomerQuery(request);
            using (var cnn = new SqlConnection(_connectionStrings.ConnectionString))
            {
                var result = await cnn.QueryAsync<SalesLogByProductDto>(cmd);

                return result;
            }
        }
    }

    public class GetAllProductsByCustomerForSalesPersonQueryHandler : ICommandHandler<GetAllProductsByCustomerForSalesPersonQuery, SyncPagedResult<SalesLogByProductDto>>
    {
        private readonly IRepository<Invoice, Guid> _invoiceRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ConnectionStrings _connectionStrings;

        public GetAllProductsByCustomerForSalesPersonQueryHandler(IRepository<Invoice, Guid> invoiceRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper, ICommandBus commandBus,
            ICurrentUser currentUser,
            ConnectionStrings connectionStrings)
        {
            _invoiceRepository = invoiceRepository;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _commandBus = commandBus;
            _connectionStrings = connectionStrings;
        }

        public async Task<SyncPagedResult<SalesLogByProductDto>> Handle(GetAllProductsByCustomerForSalesPersonQuery request, CancellationToken cancellationToken)
        {
            var pagedQueryBuilder = new PagedQueryBuilder();
            var sqlCmd = pagedQueryBuilder.BuildInvoiceByProductsOfCustomerQuery(request);
            var sqlCmdTotal = pagedQueryBuilder.BuildTotalRecords(request);
            using (var cnn = new SqlConnection(_connectionStrings.ConnectionString))
            {
                var result = await cnn.QueryAsync<SalesLogByProductDto>(sqlCmd);

                var total = await cnn.QueryFirstOrDefaultAsync<Int32>(sqlCmdTotal);
                return new SyncPagedResult<SalesLogByProductDto> { Count = total, Result = result };
            }

        }


    }
}