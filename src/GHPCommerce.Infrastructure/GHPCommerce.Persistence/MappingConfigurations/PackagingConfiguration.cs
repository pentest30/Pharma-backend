using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class PackagingConfiguration : IEntityTypeConfiguration<Packaging>
    {
        public void Configure(EntityTypeBuilder<Packaging> builder)
        {
            builder.ToTable("Packaging", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Products);
        }
    }
}
