using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using GHPCommerce.Core.Shared.PreparationOrder.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Persistence;
using Microsoft.Data.SqlClient;
namespace GHPCommerce.Modules.PreparationOrder.Queries
{
    public class GetPOItemsQueryHandler : ICommandHandler<GetPOItemsQuery, List<PreparationOrderItemDtoV1>>
    {
        
        private readonly ConnectionStrings _connectionStrings;

        public GetPOItemsQueryHandler( ConnectionStrings connectionStrings)
        {
            _connectionStrings = connectionStrings;
        }
        public async Task<List<PreparationOrderItemDtoV1>> Handle(GetPOItemsQuery request, CancellationToken cancellationToken)
        {
            var sqlCmd = @"SELECT SUM(quantity) as Quantity
                          ,[ProductName]
                          ,[ProductCode]
                          ,[InternalBatchNumber]
	                      ,OrderNumberSequence
                      FROM [logistics].[PreparationOrderItem] as poi
                      join  logistics.PreparationOrder as op on op.Id = poi.PreparationOrderId
                      left join  sales.Orders as o on op.OrderId = o.Id
                      where poi.IsControlled = 0
                      group by [InternalBatchNumber], ProductName ,[ProductCode],[InternalBatchNumber],OrderNumberSequence
	                  order by ProductName";
            using (var cnn = new SqlConnection(_connectionStrings.ConnectionString))
            {
                var result = await cnn.QueryAsync<PreparationOrderItemDtoV1>(sqlCmd);
                return result.ToList();
            }
        }
    }
}