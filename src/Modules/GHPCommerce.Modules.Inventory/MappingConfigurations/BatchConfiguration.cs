using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    public class BatchConfiguration: IEntityTypeConfiguration<Batch>
    {
        public void Configure(EntityTypeBuilder<Batch> builder)
        {
            builder.ToTable("Batch", schema: "inventory");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.Property(m => m.SalesDiscountRatio).HasDefaultValue((float?)0);
            builder.Property(m => m.PurchaseDiscountRatio).HasDefaultValue((float?)0);
            builder.Property(m => m.PFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaHT).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaPFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaTTC).HasColumnType("decimal(18,2)");
            builder.HasMany(x => x.Invents)
               .WithOne(x => x.Batch)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}