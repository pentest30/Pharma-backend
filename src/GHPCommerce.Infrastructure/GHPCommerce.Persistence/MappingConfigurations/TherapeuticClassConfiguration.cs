using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class TherapeuticClassConfiguration : IEntityTypeConfiguration<TherapeuticClass>
    { 
        public void Configure(EntityTypeBuilder<TherapeuticClass> builder)
        {
            builder.ToTable("TherapeuticClasses", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Products);
        }
    }
}