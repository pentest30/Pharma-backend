using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    public class InventConfiguration : IEntityTypeConfiguration<Invent>
    {
        public void Configure(EntityTypeBuilder<Invent> builder)
        {
            builder.ToTable("Invent", schema: "inventory");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.Property(x => x.PhysicalReservedQuantity).HasDefaultValue(0);
            builder.HasIndex(x => new
            {
                x.OrganizationId,
                x.ProductId,
                x.VendorBatchNumber,
                x.InternalBatchNumber,
                x.Color,
                x.Size,
                x.ZoneId,
                x.StockStateId
            }).HasName("IX_InventoryDimensions").IsClustered(false).IsUnique().HasFilter(null);
            builder.Property(m => m.SalesDiscountRatio).HasDefaultValue((float?)0);
            builder.Property(m => m.PurchaseDiscountRatio).HasDefaultValue((float?)0);
            builder.Property(m => m.PFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaHT).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaPFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaTTC).HasColumnType("decimal(18,2)");
            builder.HasMany(x => x.InventItemTransactions)
           .WithOne(x => x.Invent)
           .OnDelete(DeleteBehavior.Cascade)
           ;
           
        }
    }
}
