using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using GHPCommerce.Domain.Repositories;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GHPCommerce.Persistence
{
   
    public sealed class ApplicationDbContext : DbContext, IUnitOfWork, IDataProtectionKeyContext
    {
        private IDbContextTransaction _dbContextTransaction;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseEFCoreLogger();
            base.OnConfiguring(optionsBuilder);
        }
        public IDbContextTransaction GetDbContextTransaction()
        {
            return _dbContextTransaction;
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
            base.OnModelCreating(builder);
         
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    }
}
