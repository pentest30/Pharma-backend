using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Core.Shared.Contracts.Inventory;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Commands;
using GHPCommerce.Modules.Sales.Commands.Orders;
using Moq;
using Xunit;

namespace GHPCommerce.Modules.Sales.Test.Commands
{
    public class UpdateOrderForB2BCustomerTest:  TestBase
    {
        public UpdateOrderForB2BCustomerTest(ServiceFixture fixture) : base(fixture){}

        [Fact]
        public async Task Update_OrderItem_With_Same_Quantity_Should_Not_Be_Allowed()
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
            moqCache.Setup(m =>
                    m.GetAsync<CachedOrder>(
                        "12837d3d-793f-ea11-becb-5cea1d05f66512837d3d-793f-ea11-becb-5cea1d05f660", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CachedOrder
                {
                    Id = Guid.NewGuid(),
                    SupplierId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"),
                    CreatedByUserId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F660"),
                    OrderItems = new List<CachedOrderItem>
                    {
                        new CachedOrderItem
                        {

                            ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                            OrderId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                            Quantity = 10,
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
                moqUser.Object,  moqCache.Object, null, null, null);


            var command = new OrderItemUpdateCommand
            {
                SupplierOrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"),
                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                OrderId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Quantity = 10,
                OldQuantity = 0
            };
            var actualResult = await ordersCommandsHandler.Handle(command, CancellationToken.None);
            var @equals = actualResult.Should().BeOfType<ValidationResult>().Which.Errors[0].ErrorMessage
                .Equals("Quantity already reserved");
            @equals.Should().BeTrue();
            
        }
        [Fact]
        public async Task Update_OrderItem_With_Less_Quantity()
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
            moqCache.Setup(m =>
                    m.GetAsync<CachedOrder>(
                        "12837d3d-793f-ea11-becb-5cea1d05f66512837d3d-793f-ea11-becb-5cea1d05f660", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CachedOrder
                {
                    Id = Guid.NewGuid(),
                    SupplierId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"),
                    CreatedByUserId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F660"),
                    OrderItems = new List<CachedOrderItem>
                    {
                        new CachedOrderItem
                        {

                            ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                            OrderId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                            Quantity = 10,
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
                moqUser.Object, moqCache.Object, null, null,null);


            var command = new OrderItemUpdateCommand
            {
                SupplierOrganizationId = Guid.Parse("12837D3D-793F-EA11-BECB-5CEA1D05F665"),
                ProductId = Guid.Parse("697d840d-98d6-4520-9d64-e8673703f59e"),
                OrderId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Quantity = 5,
                OldQuantity = 0
            };
            var actualResult = await ordersCommandsHandler.Handle(command, CancellationToken.None);
            var stock = await moqCache.Object.GetAsync<InventSumCreatedEvent>(
                "697d840d-98d6-4520-9d64-e8673703f59e" + "12837D3D-793F-EA11-BECB-5CEA1D05F665".ToLower(),
                It.IsAny<CancellationToken>());
            stock.CachedInventSumCollection.CachedInventSums.First().PhysicalReservedQuantity.Should().Be(5);
            stock.CachedInventSumCollection.CachedInventSums.First().PhysicalAvailableQuantity.Should().Be(45);

            var order = await moqCache.Object.GetAsync<CachedOrder>(
                "12837d3d-793f-ea11-becb-5cea1d05f66512837d3d-793f-ea11-becb-5cea1d05f660", It.IsAny<CancellationToken>());
             order?.OrderItems.Count.Should().Be(1);
             order?.OrderItems.First().Quantity.Should().Be(5);
           

        }
    }
}
