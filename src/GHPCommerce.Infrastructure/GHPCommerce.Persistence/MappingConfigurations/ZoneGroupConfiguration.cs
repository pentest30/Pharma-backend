using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    class ZoneGroupConfiguration : IEntityTypeConfiguration<ZoneGroup>
    {
        public void Configure(EntityTypeBuilder<ZoneGroup> builder)
        {
            builder.ToTable("ZoneGroup", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.PickingZones);
        }
    }
}
