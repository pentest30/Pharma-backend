using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class INNCodeDosageConfiguration : IEntityTypeConfiguration<INNCodeDosage>
    {
        public void Configure(EntityTypeBuilder<INNCodeDosage> builder)
        {
            builder.ToTable("INNCodeDosage", schema: "Catalog");
            builder.HasKey(x => new { x.DosageId, x.INNCodeId, x.INNId });
        }
    }
}