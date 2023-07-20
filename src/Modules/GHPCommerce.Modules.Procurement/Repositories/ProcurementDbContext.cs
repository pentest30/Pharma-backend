using System;
using System.Data;
using System.Reflection;
using System.Threading.Tasks;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GHPCommerce.Modules.Procurement.Repositories
{
    public class ProcurementDbContext : DbContext, IUnitOfWork
    {
        private IDbContextTransaction _dbContextTransaction;

        public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options)
            : base(options)
        {
            
        }
       
        public Task SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

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
            throw new NotImplementedException();
        }
        public IDbContextTransaction GetDbContextTransaction()
        {
            BeginTransaction();
            return  _dbContextTransaction;
        }
    }
}