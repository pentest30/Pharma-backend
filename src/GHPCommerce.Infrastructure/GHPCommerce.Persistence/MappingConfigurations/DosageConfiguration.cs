using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class DosageConfiguration : IEntityTypeConfiguration<Dosage>
    {
        public void Configure(EntityTypeBuilder<Dosage> builder)
        {
            builder.ToTable("Dosages", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.InnCodeDosages);
        }
    }
}