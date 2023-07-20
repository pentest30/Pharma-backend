using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands", schema:"Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Products);
        }
    }
}