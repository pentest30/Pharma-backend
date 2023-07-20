using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Modules.Inventory.MappingConfigurations
{
    public class ArrivalsConfiguration: IEntityTypeConfiguration<Arrivals>
    {
        public void Configure(EntityTypeBuilder<Arrivals> builder)
        {
            builder.ToView("DW_VIEW_ARRIVAGE");
            builder.HasNoKey();
        }
    }
}