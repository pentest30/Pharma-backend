using GHPCommerce.Modules.Quota.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Quota.MappingConfigurations
{
    public class QuotaInitStateConfiguration: IEntityTypeConfiguration<QuotaInitState>
    {
        public void Configure(EntityTypeBuilder<QuotaInitState> builder)
        {
            builder.ToTable("QuotaInitState", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");

        }
    }
}