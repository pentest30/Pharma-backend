using GHPCommerce.Modules.Quota.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Quota.MappingConfigurations
{
    public class QuotaTransactionConfiguration: IEntityTypeConfiguration<QuotaTransaction>
    {
        public void Configure(EntityTypeBuilder<QuotaTransaction> builder)
        {
            builder.ToTable("QuotaTransaction", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
         
        }
    }
}