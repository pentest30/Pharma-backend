﻿// <auto-generated />
using System;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GHPCommerce.Migrator.Migrations.PreparationOrderDb
{
    [DbContext(typeof(PreparationOrderDbContext))]
    partial class PreparationOrderDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.ConsolidationOrder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<bool>("Consolidated")
                        .HasColumnType("bit");

                    b.Property<Guid?>("ConsolidatedById")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConsolidatedByName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ConsolidatedTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmployeeCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrderIdentifier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrganizationName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PrintCount")
                        .HasColumnType("int");

                    b.Property<bool>("Printed")
                        .HasColumnType("bit");

                    b.Property<Guid?>("PrintedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PrintedByName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("PrintedTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReceivedInShippingBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ReceivedInShippingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("ReceptionExpedition")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ReceptionExpeditionTime")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<bool>("SentForExpedition")
                        .HasColumnType("bit");

                    b.Property<int>("TotalPackage")
                        .HasColumnType("int");

                    b.Property<int>("TotalPackageThermolabile")
                        .HasColumnType("int");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("ConsolidationOrder","logistics");
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.DeleiveryOrder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DeleiveryOrderDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("SequenceNumber")
                        .HasColumnType("int");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TotalPackage")
                        .HasColumnType("int");

                    b.Property<int>("TotalPackageThermolabile")
                        .HasColumnType("int");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<bool>("Validated")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("DeleiveryOrder","logistics");
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.DeleiveryOrderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("DeleiveryOrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Discount")
                        .HasColumnType("float");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<double>("ExtraDiscount")
                        .HasColumnType("float");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("PFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Packing")
                        .HasColumnType("int");

                    b.Property<decimal>("PpaHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("PurchaseUnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<double>("Tax")
                        .HasColumnType("float");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("VendorBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DeleiveryOrderId");

                    b.ToTable("DeleiveryOrderItem","logistics");
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<string>("BarCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("BarCodeImage")
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("CodeAx")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ConsolidatedById")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ConsolidatedByName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ConsolidatedTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("EmployeeCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IdentifierNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrderIdentifier")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrganizationName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PreparationOrderNumberSequence")
                        .HasColumnType("int");

                    b.Property<long>("PreparationOrderStatus")
                        .HasColumnType("bigint");

                    b.Property<int>("PrintCount")
                        .HasColumnType("int");

                    b.Property<bool>("Printed")
                        .HasColumnType("bit");

                    b.Property<Guid?>("PrintedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PrintedByName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("PrintedTime")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("SectorCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SectorName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SequenceNumber")
                        .HasColumnType("int");

                    b.Property<bool>("ToBeRespected")
                        .HasColumnType("bit");

                    b.Property<int>("TotalPackage")
                        .HasColumnType("int");

                    b.Property<int>("TotalPackageThermolabile")
                        .HasColumnType("int");

                    b.Property<int>("TotalZoneCount")
                        .HasColumnType("int");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("ZoneDoneCount")
                        .HasColumnType("int");

                    b.Property<Guid>("ZoneGroupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ZoneGroupName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ZoneGroupOrder")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("PreparationOrder","logistics");
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderExecuter", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("ExecutedById")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ExecutedByName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExecutedTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("PickingZoneId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PickingZoneName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("PreparationOrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("PreparationOrderId");

                    b.ToTable("PreparationOrderExecuter","logistics");
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("DefaultLocation")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Discount")
                        .HasColumnType("float");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<double>("ExtraDiscount")
                        .HasColumnType("float");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsControlled")
                        .HasColumnType("bit");

                    b.Property<double>("OldDiscount")
                        .HasColumnType("float");

                    b.Property<int?>("OldQuantity")
                        .HasColumnType("int");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Packing")
                        .HasColumnType("int");

                    b.Property<int?>("PackingQuantity")
                        .HasColumnType("int");

                    b.Property<Guid?>("PickingZoneId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PickingZoneName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PickingZoneOrder")
                        .HasColumnType("int");

                    b.Property<decimal>("PpaHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("PreparationOrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PreviousInternalBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<long>("Status")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("VendorBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("ZoneGroupId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ZoneGroupName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("PreparationOrderId");

                    b.ToTable("PreparationOrderItem","logistics");
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderVerifier", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("PickingZoneId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PickingZoneName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("PreparationOrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("VerifiedById")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("VerifiedByName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("VerifiedTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("PreparationOrderId");

                    b.ToTable("PreparationOrderVerifier","logistics");
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.DeleiveryOrderItem", b =>
                {
                    b.HasOne("GHPCommerce.Modules.PreparationOrder.Entities.DeleiveryOrder", "DeleiveryOrder")
                        .WithMany("DeleiveryOrderItems")
                        .HasForeignKey("DeleiveryOrderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderExecuter", b =>
                {
                    b.HasOne("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrder", null)
                        .WithMany("PreparationOrderExecuters")
                        .HasForeignKey("PreparationOrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderItem", b =>
                {
                    b.HasOne("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrder", "PreparationOrder")
                        .WithMany("PreparationOrderItems")
                        .HasForeignKey("PreparationOrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrderVerifier", b =>
                {
                    b.HasOne("GHPCommerce.Modules.PreparationOrder.Entities.PreparationOrder", null)
                        .WithMany("PreparationOrderVerifiers")
                        .HasForeignKey("PreparationOrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
