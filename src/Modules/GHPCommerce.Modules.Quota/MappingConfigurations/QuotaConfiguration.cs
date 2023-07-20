using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Quota.MappingConfigurations
{
    public class QuotaConfiguration : IEntityTypeConfiguration<Entities.Quota>
    {
        public void Configure(EntityTypeBuilder<Entities.Quota> builder)
        {
            builder.ToTable("Quota", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.QuotaTransactions)
                .WithOne(x => x.Quota)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}