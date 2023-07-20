using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class TaxGroupConfiguration : IEntityTypeConfiguration<TaxGroup>
    {
        public void Configure(EntityTypeBuilder<TaxGroup> builder)
        {
            builder.ToTable("TaxGroups", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Products);
            builder.Property(m => m.TaxValue).HasColumnType("decimal(18,2)");
        }
    }
}
