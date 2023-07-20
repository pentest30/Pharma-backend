using GHPCommerce.Modules.Sales.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Sales.MappingConfigurations
{
    public class InvoiceItemConfiguration: IEntityTypeConfiguration<InvoiceItem>
    {
        public void Configure(EntityTypeBuilder<InvoiceItem> builder)
        {
            builder.ToTable("InvoiceItems", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");//.ValueGeneratedNever();
            builder.Property(m => m.UnitPriceInclTax).HasColumnType("decimal(18,2)");
            builder.Property(m => m.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.TotalExlTax).HasColumnType("decimal(18,2)");
            builder.Property(m => m.TotalInlTax).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaHT).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaTTC).HasColumnType("decimal(18,2)");

        }
    }
}