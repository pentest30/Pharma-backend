using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Persistence;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Modules.Sales.DTOs;
using Microsoft.Data.SqlClient;

namespace GHPCommerce.Modules.Sales.Queries.Invoices
{
    public class GetTurnoversForAllCustomersQuery : ICommand<Dictionary<string ,List<TurnoverDto>>>
    {
        public DateTime? Period { get; set; }
    }
    public class GetTurnoversForAllCustomersQueryHandler : ICommandHandler<GetTurnoversForAllCustomersQuery,Dictionary<string ,List<TurnoverDto>> >
    {
        private readonly ICommandBus _commandBus;
        private readonly ConnectionStrings _connectionStrings;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICurrentUser _currentUser;

        public GetTurnoversForAllCustomersQueryHandler(ICommandBus commandBus,
            ConnectionStrings connectionStrings, 
            ICurrentOrganization currentOrganization, 
            ICurrentUser currentUser)
        {
            _commandBus = commandBus;
            _connectionStrings = connectionStrings;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
        }
        public async Task<Dictionary<string ,List<TurnoverDto>>> Handle(GetTurnoversForAllCustomersQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null)
                return default;
            var currentUser = await _commandBus.SendAsync(new GetUserQuery {Id = _currentUser.UserId, IncludeRoles = true},
                cancellationToken);
            TurnoverQueryBuilder queryBuilder = new TurnoverQueryBuilder();
            var unionSql = queryBuilder.BuildTurnOverSqlQuery(currentUser, request.Period ?? DateTime.Now);
            using (var cnn = new SqlConnection(_connectionStrings.ConnectionString))
            {
                var result = await cnn.QueryAsync<TurnoverDto>(unionSql);
                var finalReport = GetFinalReport(result);
                return finalReport;
            }
        }

        private static Dictionary<string, List<TurnoverDto>> GetFinalReport(IEnumerable<TurnoverDto> result)
        {
            var finalResult = new List<TurnoverDto>();
            foreach (var turnoverDtos in result.GroupBy(x => x.CustomerName))
            {
                for (int i = 0; i < turnoverDtos.Count(); i++)
                {
                    var item = turnoverDtos.ToList()[i];
                    if (finalResult.All(x => x.CustomerName != turnoverDtos.ToList()[i].CustomerName))
                    {
                        var turnoverDto = new TurnoverDto();
                        turnoverDto.CustomerName = item.CustomerName;
                        //  turnoverDto.MonthlyTurnover = item.MonthlyTurnover;
                        turnoverDto.SalesPersonName = item.SalesPersonName;
                        turnoverDto.SalesGroup = item.SalesGroup;
                        if (item.ReportType == "daily")
                            turnoverDto.DailyTurnover = item.Turnover;
                        if (item.ReportType == "monthly")
                            turnoverDto.MonthlyTurnover = item.Turnover;
                        if (item.ReportType == "quarter")
                            turnoverDto.QuarterTurnover = item.Turnover;
                        finalResult.Add(turnoverDto);
                    }
                    else
                    {
                        var turnoverDto = finalResult.FirstOrDefault(x =>
                            x.CustomerName == turnoverDtos.ToList()[i].CustomerName);
                        if (turnoverDto != null)
                        {
                            if (item.ReportType == "daily")
                                turnoverDto.DailyTurnover += item.Turnover;
                            if (item.ReportType == "monthly")
                                turnoverDto.MonthlyTurnover += item.Turnover;
                            if (item.ReportType == "quarter")
                                turnoverDto.QuarterTurnover += item.Turnover;
                        }
                    }
                }
            }

            var secondReport = new List<TurnoverDto>();
            foreach (var turnoverDto in finalResult)
            {
                turnoverDto.SalesGroup ??= turnoverDto.SalesPersonName;
            }
            foreach (var turnoverDto in finalResult.DeepCloneJson().GroupBy(x => x.SalesGroup))
            {
                var item = turnoverDto.FirstOrDefault();
                if (item != null)
                {
                    item.DailyTurnover = turnoverDto.Sum(x => x.DailyTurnover);
                    item.MonthlyTurnover = turnoverDto.Sum(x => x.MonthlyTurnover);
                    item.QuarterTurnover = turnoverDto.Sum(x => x.QuarterTurnover);
                    secondReport.Add(item);
                }
            }

            var thirdReport = new List<TurnoverDto>();
            foreach (var turnoverDto in finalResult.DeepCloneJson().GroupBy(x => x.SalesPersonName))
            {
                var item = turnoverDto.FirstOrDefault();
                if (item != null)
                {
                    item.DailyTurnover = turnoverDto.Sum(x => x.DailyTurnover);
                    item.MonthlyTurnover = turnoverDto.Sum(x => x.MonthlyTurnover);
                    item.QuarterTurnover = turnoverDto.Sum(x => x.QuarterTurnover);
                    thirdReport.Add(item);
                }
            }

            Dictionary<string, List<TurnoverDto>> finalReport = new Dictionary<string, List<TurnoverDto>>();
            finalReport.Add("first_report", finalResult.OrderByDescending(x => x.DailyTurnover).ToList());
            finalReport.Add("second_report", secondReport.OrderByDescending(x => x.DailyTurnover).ToList());
            finalReport.Add("third_report", thirdReport.OrderByDescending(x => x.DailyTurnover).ToList());
            return finalReport;
        }
    }
}