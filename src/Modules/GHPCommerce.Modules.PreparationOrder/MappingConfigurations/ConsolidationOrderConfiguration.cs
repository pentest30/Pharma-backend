using GHPCommerce.Modules.PreparationOrder.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.PreparationOrder.MappingConfigurations
{
    public class ConsolidationOrderConfiguration : IEntityTypeConfiguration<ConsolidationOrder>
    {
        public void Configure(EntityTypeBuilder<ConsolidationOrder> builder)
        {
            builder.ToTable("ConsolidationOrder", schema: "logistics");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
        }
    }
}
