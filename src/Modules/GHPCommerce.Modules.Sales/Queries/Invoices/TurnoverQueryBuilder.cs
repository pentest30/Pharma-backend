using System;
using System.Linq;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public class TurnoverQueryBuilder
    {
   
        public string BuildTurnOverSqlQuery(User currentUser, DateTime dateTime)
        {
            string unionSql = "";
            if (currentUser.UserRoles.Any(x => x.Role.Name == "Admin"))
            {
                unionSql = $"{DailySqlQuery(dateTime, "daily", null, null)}" +
                           " UNION ALL" +
                           $" {MonthlyAndQuarterQuery(dateTime.FirstDayOfMonth(), dateTime.LastDayOfMonth(), "monthly", null, null)}"
                           + " UNION ALL" +
                           $" {MonthlyAndQuarterQuery(dateTime.GetFirstDayOfQuarter(), dateTime.GetLastDayOfQuarter(), "quarter", null, null)}"
                           + " order by Turnover desc";
            }

            else if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson") && currentUser.UserRoles.All(x => x.Role.Name != "Supervisor"))
            {
                unionSql = $"{DailySqlQuery(dateTime, "daily", currentUser.Id, null)}" +
                           " UNION ALL" +
                           $" {MonthlyAndQuarterQuery(dateTime.FirstDayOfMonth(), dateTime.LastDayOfMonth(), "monthly", currentUser.Id, null)}"
                           + " UNION ALL" +
                           $" {MonthlyAndQuarterQuery(dateTime.GetFirstDayOfQuarter(), dateTime.GetLastDayOfQuarter(), "quarter", currentUser.Id, null)}"
                           + " order by Turnover desc";
            }
            else if (currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor") )
            {
                unionSql = $"{DailySqlQuery(dateTime, "daily", currentUser.Id, currentUser.Id)}" +
                           " UNION ALL" +
                           $" {MonthlyAndQuarterQuery(dateTime.FirstDayOfMonth(), dateTime.LastDayOfMonth(), "monthly", currentUser.Id, currentUser.Id)}"
                           + " UNION ALL" +
                           $" {MonthlyAndQuarterQuery(dateTime.GetFirstDayOfQuarter(), dateTime.GetLastDayOfQuarter(), "quarter", currentUser.Id, currentUser.Id)}"
                           + " order by Turnover desc";
            }

            return unionSql;
        }

        private string DailySqlQuery(DateTime firstDay, string type, Guid? salesPersonId, Guid? managerId)
        {
            var  sqlCmd = @$"select SUM(OrderDiscount) as Turnover,CustomerName, reportType = '{type}',s.MonthlyObjective, o.CreatedBy as SalesPersonName ,us.ManagerId ,(select UserName from ids.Users where id = us.ManagerId) as SalesGroup  from sales.Orders as o
						left join Tiers.Organizations as org on org.Id = CustomerId
						left join Tiers.Customers as c on c.OrganizationId = org.Id
						left join Tiers.SupplierCustomers as s on s.CustomerId = c.Id
                        left join ids.Users as us on us.id = o.CreatedByUserId
                        where o.CreatedDateTime >= '{firstDay.Date.ToShortDateString()}'
                          and o.CreatedDateTime < '{firstDay.Date.AddDays(1).ToShortDateString()}'
                          and orderstatus <> 70";
            if (salesPersonId.HasValue)
            {
                sqlCmd += @$"  and o.CreatedByUserId = '{salesPersonId.Value}'";
            }

            if (managerId.HasValue)
            {
                sqlCmd += @$" and  o.CreatedByUserId in (select id  from us where  us.id = '{managerId.Value}')";
            }
            sqlCmd +=" group by  CustomerName,s.MonthlyObjective, o.CreatedBy,us.ManagerId ";
            return sqlCmd;
        }
        private string MonthlyAndQuarterQuery(DateTime firstDay, DateTime lastDay, string type, Guid? salesPersonId, Guid? managerId)
        {
            var sqlCmd =
                @$"select SUM(TotalHT) as Turnover,i.CustomerName, reportType = '{type}',s.MonthlyObjective ,o.CreatedBy as SalesPersonName,us.ManagerId ,(select UserName from ids.Users where id = us.ManagerId) as SalesGroup    from sales.Invoices as i
                        left join Tiers.Customers as c on c.id = i.customerid
						left join Tiers.SupplierCustomers as s on s.CustomerId = c.Id
                        left join sales.Orders as o on o.CodeAx = 'DEF-BCC/'+Right(Year(i.InvoiceDate),2)+'-'+ + Right('000000'+ Convert(varchar, i.OrderNumber), 6)
                        left join ids.Users as us on us.id = o.CreatedByUserId
                        where i.CreatedDateTime >= '{firstDay.Date.ToShortDateString()}'
                        and i.CreatedDateTime <= '{lastDay.Date.ToShortDateString()}' ";
            if (salesPersonId.HasValue)
            {
                sqlCmd += @$"  and o.CreatedByUserId = '{salesPersonId.Value}'";
            }
            if (managerId.HasValue)
            {
                sqlCmd += @$" and  o.CreatedByUserId in (select id  from us where  us.id = '{managerId.Value}')";
            }
            sqlCmd +=" group by  i.CustomerName,s.MonthlyObjective,o.CreatedBy,us.ManagerId";
            return sqlCmd;
        }
    }
}