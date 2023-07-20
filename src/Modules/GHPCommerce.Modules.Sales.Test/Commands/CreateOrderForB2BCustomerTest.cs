using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using Moq;
using Xunit;
using FluentAssertions;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using GHPCommerce.Modules.Sales.Commands.Orders;

namespace GHPCommerce.Modules.Sales.Test.Commands
{
    public class CreateOrderForB2BCustomerTest : TestBase
    {

        [Fact]
        public async Task OrderCommand_Should_Be_Created()
        {
            var moqCache = new Mock<ICache>();
            moqCache.Setup(m =>
                    m.GetAsync<InventSumCreatedEvent>(
                        "697d840d-98d6-4520-9d64-e8673703f59e" + "12837D3D-793F-EA11-BECB-5CEA1D05F665".ToLower(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InventSumCreatedEvent
                {
                    CachedInventSumCollection = new CachedInventSumCollection
                    {
                        CachedInventSums =
                        {
                            new CachedInventSum
                            {
                                ExpiryDate = DateTime.Now.AddYears(1),
                                InternalBatchNumber = "LOT123654987",
                                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                                PhysicalOnhandQuantity = 50,
                                IsPublic = true,
                                SalesUnitPrice = 150,
                                PurchaseUnitPrice = 120,
                                VendorBatchNumber = "LOT1470098714",
                                OrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665")
                            }
                        }
                    }
                });
            var moqUser = new Mock<ICurrentUser>();
            moqUser.Setup(s => s.UserId).Returns(() => Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F660"));
            var moqCommandBus = new Mock<ICommandBus>();
            moqCommandBus.Setup(m => m.SendAsync(It.IsAny<GetProductById>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductDtoV3
                {
                    Available = true,
                    Id = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                    FullName = "PARACETAMOL",
                    Code = "LOT123654"
                });
            var moqOrg = new Mock<ICurrentOrganization>();
            moqOrg.Setup(s => s.GetCurrentOrganizationIdAsync())
                .ReturnsAsync(() => Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"));

            var ordersCommandsHandler = new OrdersCommandsHandler(null, Mapper, moqOrg.Object, moqCommandBus.Object,
                moqUser.Object, moqCache.Object, null, null, null);


            var command = new OrderItemCreateCommand
            {
                SupplierOrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"),
                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                OrderId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Quantity = 10,
                OldQuantity = 0
            };
            var actualResult = await ordersCommandsHandler.Handle(command, CancellationToken.None);

            Assert.Null(actualResult);
        }

        [Fact]
        public async Task OrderCommand_With_Expired_Stock_Should_Not_Be_Created()
        {
            var moqCache = new Mock<ICache>();
            moqCache.Setup(m =>
                    m.GetAsync<InventSumCreatedEvent>(
                        "697d840d-98d6-4520-9d64-e8673703f59e" + "12837D3D-793F-EA11-BECB-5CEA1D05F665".ToLower(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InventSumCreatedEvent
                {
                    CachedInventSumCollection = new CachedInventSumCollection
                    {
                        CachedInventSums =
                        {
                            new CachedInventSum
                            {
                                ExpiryDate = DateTime.Now.AddYears(-1),
                                InternalBatchNumber = "LOT123654987",
                                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                                PhysicalOnhandQuantity = 50,
                                IsPublic = true,
                                SalesUnitPrice = 150,
                                PurchaseUnitPrice = 120,
                                VendorBatchNumber = "LOT1470098714",
                                OrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665")
                            }
                        }
                    }
                });
            var moqUser = new Mock<ICurrentUser>();
            moqUser.Setup(s => s.UserId).Returns(() => Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F660"));
            var moqCommandBus = new Mock<ICommandBus>();
            moqCommandBus.Setup(m => m.SendAsync(It.IsAny<GetProductById>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductDtoV3
                {
                    Available = true,
                    Id = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                    FullName = "PARACETAMOL",
                    Code = "LOT123654"
                });
            var moqOrg = new Mock<ICurrentOrganization>();
            moqOrg.Setup(s => s.GetCurrentOrganizationIdAsync())
                .ReturnsAsync(() => Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"));

            var ordersCommandsHandler = new OrdersCommandsHandler(null, Mapper, moqOrg.Object, moqCommandBus.Object,
                moqUser.Object, moqCache.Object, null, null, null);


            var command = new OrderItemCreateCommand
            {
                SupplierOrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"),
                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                OrderId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Quantity = 10,
                OldQuantity = 0
            };
            var actualResult = await ordersCommandsHandler.Handle(command, CancellationToken.None);

            var result = actualResult.Should().BeOfType<ValidationResult>().Which.Errors[0].ErrorMessage
                .Equals("Stock non disponible");
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Concurrent_Order_Creation_Should_Not_Be_Allowed()
        {
            var moqCache = new Mock<ICache>();
            moqCache.Setup(m =>
                    m.GetAsync<InventSumCreatedEvent>(
                        "697d840d-98d6-4520-9d64-e8673703f59e" + "12837D3D-793F-EA11-BECB-5CEA1D05F665".ToLower(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InventSumCreatedEvent
                {
                    CachedInventSumCollection = new CachedInventSumCollection
                    {
                        CachedInventSums =
                        {
                            new CachedInventSum
                            {
                                ExpiryDate = DateTime.Now.AddYears(1),
                                InternalBatchNumber = "LOT123654987",
                                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                                PhysicalOnhandQuantity = 120,
                                IsPublic = true,
                                SalesUnitPrice = 120,
                                PurchaseUnitPrice = 120,
                                VendorBatchNumber = "LOT1470098714",
                                OrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665")
                            }
                        }
                    }
                });

            var moqUser = new Mock<ICurrentUser>();
            moqUser.Setup(s => s.UserId).Returns(() => Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F660"));
            var moqCommandBus = new Mock<ICommandBus>();
            moqCommandBus.Setup(m => m.SendAsync(It.IsAny<GetProductById>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductDtoV3
                {
                    Available = true,
                    Id = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                    FullName = "PARACETAMOL",
                    Code = "12365LO4"
                });
            var moqOrg = new Mock<ICurrentOrganization>();
            moqOrg.Setup(s => s.GetCurrentOrganizationIdAsync())
                .ReturnsAsync(() => Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"));
            var ordersCommandsHandler = new OrdersCommandsHandler(null, Mapper, moqOrg.Object, moqCommandBus.Object,
                moqUser.Object, moqCache.Object, null, null, null);


            var command = new OrderItemCreateCommand
            {
                SupplierOrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"),
                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                OrderId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Quantity = 40,
                OldQuantity = 0
            };
            var tasks = new List<Task<ValidationResult>>();
            for (int i = 0; i < 4; i++)
                tasks.Add(ordersCommandsHandler.Handle(command, CancellationToken.None));

            var results = await Task.WhenAll(tasks);
            for (int i = 0; i < results.Length; i++)
            {
                if (i <= 2) results[i].Should().Be(default);
                else
                {
                    var @equals = results[i].Should().BeOfType<ValidationResult>().Which.Errors[0].ErrorMessage
                        .Equals("La ligne n'a pas pu être entièrement réservée, Quantité disponible = 0");
                    @equals.Should().BeTrue();
                }
            }

        }

        [Fact]
        public async Task OrderCommand_Should_Create_Tow_OrderItems()
        {
            var moqCache = new Mock<ICache>();
            moqCache.Setup(m =>
                    m.GetAsync<InventSumCreatedEvent>(
                        "697d840d-98d6-4520-9d64-e8673703f59e" + "12837D3D-793F-EA11-BECB-5CEA1D05F665".ToLower(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InventSumCreatedEvent
                {
                    CachedInventSumCollection = new CachedInventSumCollection
                    {
                        CachedInventSums =
                        {
                           
                            new CachedInventSum
                            {
                                ExpiryDate = DateTime.Now.AddYears(1),
                                InternalBatchNumber = "LOT123654989",
                                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                                PhysicalOnhandQuantity = 90,
                                IsPublic = true,
                                SalesUnitPrice = 150,
                                PurchaseUnitPrice = 120,
                                VendorBatchNumber = "LOT1470098715",
                                OrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665")
                            },
                            new CachedInventSum
                            {
                                ExpiryDate = DateTime.Now.AddDays(20),
                                InternalBatchNumber = "LOT123654987",
                                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                                PhysicalOnhandQuantity = 50,
                                IsPublic = true,
                                SalesUnitPrice = 150,
                                PurchaseUnitPrice = 120,
                                VendorBatchNumber = "LOT1470098714",
                                OrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665")
                            }
                        },
                    }
                });
            var moqUser = new Mock<ICurrentUser>();
            moqUser.Setup(s => s.UserId).Returns(() => Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F660"));
            var moqCommandBus = new Mock<ICommandBus>();
            moqCommandBus.Setup(m => m.SendAsync(It.IsAny<GetProductById>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProductDtoV3
                {
                    Available = true,
                    Id = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                    FullName = "PARACETAMOL",
                    Code = "LOT123654"
                });
            var moqOrg = new Mock<ICurrentOrganization>();
            moqOrg.Setup(s => s.GetCurrentOrganizationIdAsync())
                .ReturnsAsync(() => Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"));

            var ordersCommandsHandler = new OrdersCommandsHandler(null, Mapper, moqOrg.Object, moqCommandBus.Object,
                moqUser.Object, moqCache.Object, null, null, null);


            var command = new OrderItemCreateCommand
            {
                SupplierOrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"),
                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                OrderId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Quantity = 130,
                OldQuantity = 0
            };
            var actualResult = await ordersCommandsHandler.Handle(command, CancellationToken.None);

            actualResult.Should().BeNull();
            var stock = await moqCache.Object.GetAsync<InventSumCreatedEvent>(
                "697d840d-98d6-4520-9d64-e8673703f59e" + "12837D3D-793F-EA11-BECB-5CEA1D05F665".ToLower(),
                It.IsAny<CancellationToken>());
            for (int i = 0; i < stock.CachedInventSumCollection.CachedInventSums.Count; i++)
            {
                if (i == 0)
                {
                    stock.CachedInventSumCollection.CachedInventSums.OrderBy(x=>x.ExpiryDate).ToList()[i].PhysicalAvailableQuantity.Should().Be(0);
                    stock.CachedInventSumCollection.CachedInventSums.OrderBy(x => x.ExpiryDate).ToList()[i].PhysicalReservedQuantity.Should().Be(50);
                    continue;
                }

                stock.CachedInventSumCollection.CachedInventSums.OrderBy(x => x.ExpiryDate).ToList()[i].PhysicalAvailableQuantity.Should().Be(10);
                stock.CachedInventSumCollection.CachedInventSums.OrderBy(x => x.ExpiryDate).ToList()[i].PhysicalReservedQuantity.Should().Be(80);

            }


        }

        public CreateOrderForB2BCustomerTest(ServiceFixture fixture) : base(fixture)
        {
        }
    }
}
