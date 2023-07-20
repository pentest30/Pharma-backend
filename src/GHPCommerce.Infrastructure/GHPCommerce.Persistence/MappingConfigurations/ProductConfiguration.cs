using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.OwnsMany(x => x.Images);
            builder.Property(m => m.PublicPrice) .HasColumnType("decimal(18,2)");
            builder.Property(m => m.ReferencePrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PurchasePrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.SalePrice).HasColumnType("decimal(18,2)");

        }
    }
}
