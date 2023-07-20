using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class PharmacologicalClassConfiguration : IEntityTypeConfiguration<PharmacologicalClass>
    {
        public void Configure(EntityTypeBuilder<PharmacologicalClass> builder)
        {
            builder.ToTable("PharmacologicalClasses", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Products)
                .WithOne(x=>x.PharmacologicalClass);
        }
    }
}
