using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Quota.MappingConfigurations
{
    public class QuotaRequestConfiguration : IEntityTypeConfiguration<Entities.QuotaRequest>
    {
        public void Configure(EntityTypeBuilder<Entities.QuotaRequest> builder)
        {
            builder.ToTable("QuotaRequest", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        }
    }
}