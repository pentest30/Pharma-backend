using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Procurement.MappingConfigurations
{
    public class DeliveryReceiptItemConfiguration : IEntityTypeConfiguration<DeliveryReceiptItem>
    {
        public void Configure(EntityTypeBuilder<DeliveryReceiptItem> builder)
        {
            builder.ToTable("DeliveryReceiptItem", schema: "procurement");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");//.ValueGeneratedNever();
            builder.Property(m => m.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.SalePrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.ppa).HasColumnType("decimal(18,2)");
          
        }
    }
}