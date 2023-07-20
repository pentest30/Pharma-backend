using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GHPCommerce.Modules.Sales.Repositories
{
    public class SalesDbContext : DbContext, IUnitOfWork
    {
        private IDbContextTransaction _dbContextTransaction;

        public SalesDbContext(DbContextOptions<SalesDbContext> options)
            : base(options)
        {
        }

        public async Task SaveChangesAsync()
        {
            await base.SaveChangesAsync();
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _dbContextTransaction = Database.BeginTransaction(isolationLevel);
        }

        public void CommitTransaction()
        {
            _dbContextTransaction.Commit();
        }

        public void UserTransaction(IDbContextTransaction transaction)
        {
            Database.UseTransaction(transaction.GetDbTransaction());
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasSequence<int>("OrderNumbers", schema: "sales")
                .StartsAt(1)
                .IncrementsBy(1);
            
            base.OnModelCreating(builder);

            // tO BE REVIEWED BY iQBALL
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());           
           
        }

        public IDbContextTransaction GetDbContextTransaction()
        {
            return _dbContextTransaction;
        }
    }
}
