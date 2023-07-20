using GHPCommerce.Domain.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class LogRequestConfiguration: IEntityTypeConfiguration<LogRequest>
    {
        public void Configure(EntityTypeBuilder<LogRequest> builder)
        {
            builder.ToTable("LogRequest", schema: "Shared");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        }
    }
}