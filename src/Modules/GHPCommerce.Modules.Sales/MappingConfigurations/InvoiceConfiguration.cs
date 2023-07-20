using GHPCommerce.Modules.Sales.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Sales.MappingConfigurations
{
    public class InvoiceConfiguration: IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices", schema: "sales");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
           
        }
    }
}