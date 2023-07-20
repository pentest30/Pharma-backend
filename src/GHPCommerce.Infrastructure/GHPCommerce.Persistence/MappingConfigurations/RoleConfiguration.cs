using System;
using System.Collections.Generic;
using GHPCommerce.Domain.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GHPCommerce.Persistence.MappingConfigurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles", schema: "ids");
            builder.Property(x => x.Id).HasDefaultValueSql("newsequentialid()");

            builder.HasMany(x => x.Claims)
                .WithOne(x => x.Role)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(x => x.UserRoles)
                .WithOne(x => x.Role)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasData(new List<Role>
            {
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0208b"),
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0209b"),
                    Name = "SuperAdmin",
                    NormalizedName = "SuperAdmin".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0210b"),
                    Name = "SalesPerson",
                    NormalizedName = "SalesPerson".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0650b"),
                    Name = "SalesManager",
                    NormalizedName = "SalesManager".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eef1-9ce0-a4c3f0d0650b"),
                    Name = "Supervisor",
                    NormalizedName = "Supervisor".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0211b"),
                    Name = "BuyerGroup",
                    NormalizedName = "BuyerGroup".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0212b"),
                    Name = "TechnicalDirector",
                    NormalizedName = "TechnicalDirector".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0213b"),
                    Name = "TechnicalDirectorGroup",
                    NormalizedName = "TechnicalDirectorGroup".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0214b"),
                    Name = "Buyer",
                    NormalizedName = "Buyer".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0215b"),
                    Name = "OnlineCustomer",
                    NormalizedName = "OnlineCustomer".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("b512f030-544c-eb11-9ce0-a4c3f0d0216b"),
                    Name = "InventoryManager",
                    NormalizedName = "InventoryManager".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("d63c47de-852f-4ce7-93e1-c3db21a06d48"),
                    Name = "Executer",
                    NormalizedName = "Executer".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("ec6e3c62-a47d-4e35-bc41-cda44e16afc2"),
                    Name = "Controller",
                    NormalizedName = "Controller".ToUpper()

                },
                new Role
                {
                    Id = Guid.Parse("8a8479d1-b0c7-43d6-8896-175cbcd27af8"),
                    Name = "PrintingAgent",
                    NormalizedName = "PrintingAgent".ToUpper()

                },
                  new Role
                {
                    Id = Guid.Parse("8c2f6a37-f308-438a-b39b-1400675ac734"),
                    Name = "Consolidator",
                    NormalizedName = "Consolidator".ToUpper()

                },
            });
        }
    }
}
