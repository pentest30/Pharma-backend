using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Procurement.MappingConfigurations
{
    public class SupplierOrderItemConfiguration : IEntityTypeConfiguration<SupplierOrderItem>
    {
        public void Configure(EntityTypeBuilder<SupplierOrderItem> builder)
        {
            builder.ToTable("SupplierOrderItem", schema: "procurement");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.Property(m => m.UnitPrice).HasColumnType("decimal(18,2)");
           
        }
    }
}