using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Procurement.MappingConfigurations
{
    public class SupplierInvoiceConfiguration: IEntityTypeConfiguration<SupplierInvoice>
    {
        public void Configure(EntityTypeBuilder<SupplierInvoice> builder)
        {
            builder.ToTable("SupplierInvoice", schema: "procurement");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Items)
                .WithOne(x => x.Invoice)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Receipts)
                .WithOne(x => x.Invoice)
                .OnDelete(DeleteBehavior.Cascade);
         
        }
    }
}