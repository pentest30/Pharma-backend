using GHPCommerce.Modules.Sales.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Sales.MappingConfigurations
{
    public class FinancialTransactionConfiguration: IEntityTypeConfiguration<FinancialTransaction>
    {
        public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
        {
            builder.ToTable("FinancialTransactions", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
           
        }
    }
}