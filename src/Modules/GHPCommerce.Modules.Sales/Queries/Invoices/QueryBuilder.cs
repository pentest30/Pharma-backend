namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public abstract class  QueryBuilder
    {
        public abstract string BuildInvoiceByProductsOfCustomerQuery(IGetAllInvoiceProductsForSalesPersonQuery request);
    }

    public class CommonQueryBuilder : QueryBuilder
    {
        public override string BuildInvoiceByProductsOfCustomerQuery(IGetAllInvoiceProductsForSalesPersonQuery request)
        {
            var sqlCmd =
                @$"select sum(quantity) as Quantity, ProductCode, ProductName, ProductId,sum(inv.TotalExlTax)  as TotalHt, sum(inv.TotalInlTax)  as TotalTTC, man.Name as Manufacturer
                            from sales.InvoiceItems as inv
                            join sales.Invoices as iv on iv.Id = inv.InvoiceId
                            left join Catalog.Products as p on p.Id = inv.ProductId
                            left join Catalog.Manufacturers as man on p.ManufacturerId = man.Id";
            if(((IGetAllProductsByCustomerForSalesPersonQuery)request).CustomerId.HasValue)
                sqlCmd +=$" where CustomerId = '{((IGetAllProductsByCustomerForSalesPersonQuery)request).CustomerId}'";
            else sqlCmd +=" where  1 = 1 ";
            if (request.Start.HasValue)
            {
                sqlCmd += $" AND iv.CreatedDateTime >= '{request.Start.Value.ToShortDateString()}' ";
            }

            if (request.End.HasValue)
            {
                if (request.Start.HasValue && request.Start.Value == request.End.Value)
                {
                    sqlCmd += $" AND iv.CreatedDateTime < '{request.End.Value.AddDays(1).ToShortDateString()}' ";
                }
                else
                {
                    sqlCmd += $" AND iv.CreatedDateTime <= '{request.End.Value.ToShortDateString()}' ";
                }

               
            }
            
            return sqlCmd;
        }
    }
    public class GetAllQueryBuilder : QueryBuilder
    {
        public override string BuildInvoiceByProductsOfCustomerQuery(IGetAllInvoiceProductsForSalesPersonQuery request)
        {
            var common = new CommonQueryBuilder();
            var sqlCmd = common.BuildInvoiceByProductsOfCustomerQuery(request);
            sqlCmd += @$" group by ProductCode, ProductName,ProductId , man.Name";
            return sqlCmd;
        }
    }


    public class PagedQueryBuilder : QueryBuilder
    {
        public override string BuildInvoiceByProductsOfCustomerQuery(IGetAllInvoiceProductsForSalesPersonQuery request)
        {
            var common = new CommonQueryBuilder();
            var sqlCmd = common.BuildInvoiceByProductsOfCustomerQuery(request);
            sqlCmd = BuildSqlWhereCmd(request, sqlCmd);

            sqlCmd += @$" group by ProductCode, ProductName,ProductId , man.Name
                        ORDER BY TotalTTC   desc OFFSET {((IGetPagedProductsByCustomerForSalesPersonQuery) request).SyncDataGridQuery.Skip} ROWS FETCH NEXT {((IGetPagedProductsByCustomerForSalesPersonQuery) request).SyncDataGridQuery.Take} ROWS ONLY";
            return sqlCmd;
        }

        public string BuildTotalRecords(IGetAllInvoiceProductsForSalesPersonQuery request)
        {
            var common = new CommonQueryBuilder();
            var sqlCmd = common.BuildInvoiceByProductsOfCustomerQuery(request);
            sqlCmd = BuildSqlWhereCmd(request, sqlCmd);

            sqlCmd += @$" group by ProductCode, ProductName,ProductId , man.Name";
            var finalCmd= @$"select count (*) from (" + sqlCmd + " \r ) as final";
            return finalCmd;
        }
        

        private static string BuildSqlWhereCmd(IGetAllInvoiceProductsForSalesPersonQuery request, string sqlCmd)
        {
            if (((IGetPagedProductsByCustomerForSalesPersonQuery)request).SyncDataGridQuery.Where != null)
            {
                foreach (var wherePredicate in ((IGetPagedProductsByCustomerForSalesPersonQuery)request).SyncDataGridQuery
                         .Where[0].Predicates)
                {
                    if (wherePredicate.Value == null)
                        continue;
                    if (wherePredicate.Field == "productName")
                    {
                        sqlCmd += $" AND ProductName like '%{wherePredicate.Value}%'";
                    }

                    if (wherePredicate.Field == "productCode")
                    {
                        sqlCmd += $" AND productCode like '%{wherePredicate.Value}%'";
                    }

                    if (wherePredicate.Field == "manufacturer")
                    {
                        sqlCmd += $" AND man.Name like '%{wherePredicate.Value}%'";
                    }
                }
            }

            return sqlCmd;
        }
    }
}