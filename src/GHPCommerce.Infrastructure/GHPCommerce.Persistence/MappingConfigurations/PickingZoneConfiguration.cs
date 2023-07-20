using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class PickingZoneConfiguration : IEntityTypeConfiguration<PickingZone>
    {
        public void Configure(EntityTypeBuilder<PickingZone> builder)
        {
            builder.ToTable("PickingZone", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Products);
        }
    }
}
