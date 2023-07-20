using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class ProductClassConfiguration : IEntityTypeConfiguration<ProductClass>
    {
        public void Configure(EntityTypeBuilder<ProductClass> builder)
        {
            builder.ToTable("ProductClasses", schema: "Catalog");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");
            builder.HasMany(x => x.Products);
            builder.HasData(new List<ProductClass>
            {
                new ProductClass
                {
                    Id = Guid.Parse("66aed9d0-3b48-4ba3-af1c-dfe3eb79a3aa"),
                    Name = "AUTRES",
                    ParentProductClassId = null

                },
                new ProductClass
                {
                    Id = Guid.Parse("d25dfd7d-9223-494a-8391-da8034f6d2ec"),
                    Name = "COMPLEMENT ALIMENTAIRE",
                    ParentProductClassId = null

                },
                new ProductClass
                {
                    Id = Guid.Parse("350c5fd1-91b0-4ade-8a88-cc35b4f66e48"),
                    Name = "COSMETIQUE",
                    ParentProductClassId = null

                },
                new ProductClass
                {
                    Id = Guid.Parse("0b3f80f6-b5ce-4c15-80dc-b7c5f270b9e1"),
                    Name = "DISPOSITIF MEDICAL",
                    ParentProductClassId = null

                },
                new ProductClass
                {
                    Id = Guid.Parse("5722c58f-85dc-4f5b-b1fe-afd32148e147"),
                    Name = "EQUIPEMENT MEDICAL",
                    ParentProductClassId = null

                },
                new ProductClass
                {
                    Id = Guid.Parse("e8dd9b8b-40a4-47b2-8a82-50514a661d04"),
                    Name = "MEDICAMENT",
                    ParentProductClassId = null,
                    IsMedicamentClass = true

                },
                new ProductClass
                {
                    Id = Guid.Parse("e03ab546-8d0c-4394-8164-29a7c4801e13"),
                    Name = "PHYTOTHERAPIE",
                    ParentProductClassId = null

                },
                new ProductClass
                {
                    Id = Guid.Parse("8fe69e79-d5d6-48b2-8680-102fc5ae5e76"),
                    Name = "REACTIF",
                    ParentProductClassId = null

                }
            });
        }
    }
}
