using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class UserConfiguration: IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users", schema: "ids");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");

            builder.HasMany(x => x.Claims)
                .WithOne(x => x.User)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.UserRoles)
                .WithOne(x => x.User)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed
            builder.HasData(new List<User>
            {
                new User
                {
                    Id = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F660"),
                    UserName = "superadmin",
                    NormalizedUserName = "PHONG@GMAIL.COM",
                    Email = "phong@gmail.com",
                    NormalizedEmail = "PHONG@GMAIL.COM",
                    PasswordHash = "AAJXI17tYiHj1+Yu+eoa4c9jq2FyRvD5WgPxem9c9TlhYDO8jdQjgOsOd2BicXSFxQA==", // v*7Un8b4rcN@<-RN
                    SecurityStamp = "5M2QLL65J6H6VFIS7VZETKXY27KNVVYJ",
                    EmailConfirmed = true,
                },
            });
        }
    }
}
