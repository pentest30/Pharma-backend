using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Sales.MappingConfigurations
{
    public class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
    {
        public void Configure(EntityTypeBuilder<CreditNote> builder)
        {
            builder.ToTable("CreditNotes", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.CreditNoteItems)
    .WithOne(x => x.CreditNote)
    .OnDelete(DeleteBehavior.Cascade);

        }
    }
}