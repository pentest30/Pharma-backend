using System;
using System.Linq;
using FluentScheduler;
using GHPCommerce.Application.Membership.Users.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Quota.Entities;
using GHPCommerce.Modules.Quota.Repositories;
using NLog;

namespace GHPCommerce.Modules.Quota.Jobs
{
    public class ReleaseQuotaJob 
    {
        private readonly Func<QuotaDbContext> _dbContextFactory;
        private readonly ICommandBus _commandBus;
        private readonly Logger _logger;
        public ReleaseQuotaJob(Func<QuotaDbContext> dbContextFactory, ICommandBus commandBus, Logger logger)
        {
            _dbContextFactory = dbContextFactory;
            _commandBus = commandBus;
            _logger = logger;
        }
        public void Execute()
        {
            using (var ctx = _dbContextFactory.Invoke())
            {
                try
                {
                    var getSupervisors = _commandBus.SendAsync(new GetSupervisorsQueryV2 {IncludeRoles = true}).GetAwaiter().GetResult();
                    var supervisorsIds = getSupervisors.Select(x => x.Id).ToArray();
                    var quotaList = ctx.Set<Entities.Quota>().Where(x => x.QuotaDate.Date <= DateTime.Now.Date.AddDays(-2) ).ToList();
                    var l = quotaList.Where(quota => supervisorsIds.All(x => x != quota.SalesPersonId)).ToList();
                    if (l.Any())
                    {
                        ctx.RemoveRange(l);
                    }
                    var quotaInitList = ctx.Set<QuotaInitState>().Where(x => x.DistributionDate.Date <= DateTime.Now.Date.AddDays(-2));
                    if (quotaInitList.Any())
                    {
                        ctx.RemoveRange(quotaInitList.Where(x => supervisorsIds.All(p => p != x.QuotaId)));
                    }
                    ctx.SaveChangesAsync().GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _logger.Error(e.Message);
                    _logger.Error(e.InnerException?.Message);
                    //throw;
                }
            }
        }
    }
}