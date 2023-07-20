using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Procurement.MappingConfigurations
{
    public class SupplierOrderConfiguration: IEntityTypeConfiguration<SupplierOrder>
    {
        public void Configure(EntityTypeBuilder<SupplierOrder> builder)
        {
            builder.ToTable("SupplierOrder", schema: "procurement");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Invoices)
                .WithOne(x => x.Order)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.OrderItems)
                .WithOne(x => x.Order)
                .OnDelete(DeleteBehavior.Cascade);
            
        }
    }
}