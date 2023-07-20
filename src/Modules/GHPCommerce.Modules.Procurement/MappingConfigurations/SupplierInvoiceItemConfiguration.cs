using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Procurement.MappingConfigurations
{
    public class SupplierInvoiceItemConfiguration: IEntityTypeConfiguration<SupplierInvoiceItem>
    {
        public void Configure(EntityTypeBuilder<SupplierInvoiceItem> builder)
        {
            builder.ToTable("SupplierInvoiceItem", schema: "procurement");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");//.ValueGeneratedNever();
            builder.Property(m => m.SalePrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaHT).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PharmacistMargin).HasColumnType("decimal(18,2)");
            builder.Property(m => m.WholesaleMargin).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaPFS).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PurchasePriceIncDiscount).HasColumnType("decimal(18,2)");   
            builder.Property(m => m.PurchaseUnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(m => m.PpaTTC).HasColumnType("decimal(18,2)");   
        }
    }
}