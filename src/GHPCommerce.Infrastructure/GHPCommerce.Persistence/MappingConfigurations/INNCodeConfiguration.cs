using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class INNCodeConfiguration : IEntityTypeConfiguration<INNCode>
    {
        public void Configure(EntityTypeBuilder<INNCode> builder)
        {
            builder.ToTable("INNCodes", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.InnCodeDosages);
        }
    }
}