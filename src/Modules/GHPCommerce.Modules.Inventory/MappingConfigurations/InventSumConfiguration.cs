using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    public class InventSumConfiguration : IEntityTypeConfiguration<InventSum>
    {
            public void Configure(EntityTypeBuilder<InventSum> builder)
            {
                builder.ToTable("InventSum", schema: "inventory");
                builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
                builder.Property(x => x.PhysicalReservedQuantity).HasDefaultValue(0);
                builder.HasIndex(x => new
                {
                    x.OrganizationId, x.SiteId, x.WarehouseId, x.ProductId,
                    x.VendorBatchNumber, x.InternalBatchNumber, x.Color, x.Size,
                    x.IsPublic
                }).HasName("IX_InventoryDimensions").IsClustered(false).IsUnique().HasFilter(null);
                builder.HasIndex(x => new
                {
                    x.ProductCode
                }).HasName("IX_InventoryProductCode");
                builder.Property(m => m.SalesDiscountRatio).HasDefaultValue((float?)0);
                builder.Property(m => m.PurchaseDiscountRatio).HasDefaultValue((float?)0);
                builder.Property(m => m.PFS).HasColumnType("decimal(18,2)");
                builder.Property(m => m.PpaHT).HasColumnType("decimal(18,2)");
                builder.Property(m => m.PpaPFS).HasColumnType("decimal(18,2)");
                builder.Property(m => m.PpaTTC).HasColumnType("decimal(18,2)");
                
            }
    }
}
