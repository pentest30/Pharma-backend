﻿// <auto-generated />
using System;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    [DbContext(typeof(SalesDbContext))]
    [Migration("20220522143041_add-credit-note-status")]
    partial class addcreditnotestatus
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("Relational:Sequence:sales.OrderNumbers", "'OrderNumbers', 'sales', '1', '1', '', '', 'Int32', 'False'")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.Billing.FinancialTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DocumentDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("FinancialTransactionType")
                        .HasColumnType("bigint");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("RefDocument")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RefNumber")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SupplierName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("TransactionAmount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("FinancialTransactions","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.Billing.Invoice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<decimal>("Benefit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("BenefitRate")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("CodeRegion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("CustomerAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomerCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("DeliveryOrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("InvoiceDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("InvoiceType")
                        .HasColumnType("bigint");

                    b.Property<int>("NumberDueDays")
                        .HasColumnType("int");

                    b.Property<int>("NumberOfPrints")
                        .HasColumnType("int");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("OrderNumber")
                        .HasColumnType("int");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PrintedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PrintedByName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Region")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Sector")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SectorCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SequenceNumber")
                        .HasColumnType("int");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("TotalDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalDisplayedDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("TotalPackage")
                        .HasColumnType("int");

                    b.Property<int>("TotalPackageThermolabile")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalTTC")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("Invoices","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.Billing.InvoiceItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<double>("DiscountRate")
                        .HasColumnType("float");

                    b.Property<double>("DisplayedDiscountRate")
                        .HasColumnType("float");

                    b.Property<decimal>("DisplayedTotalDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("InvoiceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("LineNum")
                        .HasColumnType("int");

                    b.Property<decimal>("PFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaTTC")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("PurchaseDiscountUnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<double>("Tax")
                        .HasColumnType("float");

                    b.Property<decimal>("TotalDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalExlTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalInlTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("UnitPriceInclTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("VendorBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("InvoiceId");

                    b.ToTable("InvoiceItems","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.CreditNotes.CreditNote", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<DateTime?>("ClaimDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ClaimNote")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("ClaimReason")
                        .HasColumnType("bigint");

                    b.Property<string>("CodeRegion")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CreatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTime>("CreditNoteDate")
                        .HasColumnType("datetime2");

                    b.Property<long>("CreditNoteType")
                        .HasColumnType("bigint");

                    b.Property<string>("CustomerAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomerCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("InvoiceDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("InvoiceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("InvoiceNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumberOfPrints")
                        .HasColumnType("int");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("OrderNumber")
                        .HasColumnType("int");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PrintedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PrintedByName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ProductReturn")
                        .HasColumnType("bit");

                    b.Property<string>("Region")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Sector")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SectorCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SequenceNumber")
                        .HasColumnType("int");

                    b.Property<long>("State")
                        .HasColumnType("bigint");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("TotalDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalDisplayedDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("TotalPackage")
                        .HasColumnType("int");

                    b.Property<int?>("TotalPackageThermolabile")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalTTC")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("ValidatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ValidatedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("CreditNotes","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.CreditNotes.CreditNoteItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("CreditNoteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("DiscountRate")
                        .HasColumnType("float");

                    b.Property<double>("DisplayedDiscountRate")
                        .HasColumnType("float");

                    b.Property<decimal>("DisplayedTotalDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LineNum")
                        .HasColumnType("int");

                    b.Property<decimal>("PFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaTTC")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("PurchaseDiscountUnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<double>("Tax")
                        .HasColumnType("float");

                    b.Property<decimal>("TotalDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalExlTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalInlTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("UnitPriceInclTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("VendorBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CreditNoteId");

                    b.ToTable("CreditNoteItems","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.Discount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<decimal>("DiscountRate")
                        .HasColumnType("decimal(18,4)");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProductFullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("ThresholdQuantity")
                        .HasColumnType("int");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTime>("from")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("to")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId", "ProductId", "DiscountRate", "ThresholdQuantity", "from", "to")
                        .IsUnique()
                        .HasName("IX_SalesDiscount")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("Discount","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid?>("ApprovedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ApprovedTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("CanceledBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CanceledTime")
                        .HasColumnType("datetime2");

                    b.Property<long>("CancellationReason")
                        .HasColumnType("bigint");

                    b.Property<string>("CodeAx")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("CompletedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("CompletedTime")
                        .HasColumnType("datetime2");

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

                    b.Property<Guid?>("DefaultSalesPersonId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("DriverName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ExpectedShippingDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("GuestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("IsSpecialOrder")
                        .HasColumnType("bit");

                    b.Property<decimal>("OrderBenefit")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("OrderBenefitRate")
                        .HasColumnType("decimal(18,2)");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("OrderDiscount")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("OrderNumberSequence")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValueSql("NEXT VALUE FOR sales.OrderNumbers");

                    b.Property<long>("OrderStatus")
                        .HasColumnType("bigint");

                    b.Property<decimal>("OrderTotal")
                        .HasColumnType("decimal(18,2)");

                    b.Property<long>("OrderType")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("PaidBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("PaidDateTime")
                        .HasColumnType("datetime2");

                    b.Property<long>("PaymentStatus")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("ProcessedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ProcessingTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("QuantitiesReleased")
                        .HasColumnType("bit");

                    b.Property<string>("RefDocument")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("RejectedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("RejectedReason")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RejectedTime")
                        .HasColumnType("datetime2");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid?>("ShippedBy")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ShippingTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SupplierName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ToBeRespected")
                        .HasColumnType("bit");

                    b.Property<string>("UpdatedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("Orders","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.OrderItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<bool>("AcceptedOnAx")
                        .HasColumnType("bit");

                    b.Property<string>("Color")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Comment")
                        .HasColumnType("nvarchar(max)");

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

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("PFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("PackagingCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Packing")
                        .HasColumnType("int");

                    b.Property<Guid?>("PickingZoneId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PickingZoneName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("PickingZoneOrder")
                        .HasColumnType("int");

                    b.Property<decimal>("PpaHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaPFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaTTC")
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

                    b.Property<string>("Size")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Tax")
                        .HasColumnType("float");

                    b.Property<bool>("Thermolabile")
                        .HasColumnType("bit");

                    b.Property<decimal>("TotalExlTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("TotalInlTax")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("UnitPriceInclTax")
                        .HasColumnType("decimal(18,2)");

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

                    b.HasIndex("OrderId");

                    b.ToTable("OrderItems","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.ShoppingCartItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid?>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Discount")
                        .HasColumnType("float");

                    b.Property<Guid?>("GuestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid?>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Tax")
                        .HasColumnType("float");

                    b.Property<decimal>("Total")
                        .HasColumnType("decimal(18,2)");

                    b.Property<double>("TotalDiscount")
                        .HasColumnType("float");

                    b.Property<double>("TotalTax")
                        .HasColumnType("float");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("ShoppingCartItems","sales");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.Billing.InvoiceItem", b =>
                {
                    b.HasOne("GHPCommerce.Modules.Sales.Entities.Billing.Invoice", null)
                        .WithMany("InvoiceItems")
                        .HasForeignKey("InvoiceId");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.CreditNotes.CreditNoteItem", b =>
                {
                    b.HasOne("GHPCommerce.Modules.Sales.Entities.CreditNotes.CreditNote", null)
                        .WithMany("CreditNoteItems")
                        .HasForeignKey("CreditNoteId");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Sales.Entities.OrderItem", b =>
                {
                    b.HasOne("GHPCommerce.Modules.Sales.Entities.Order", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
