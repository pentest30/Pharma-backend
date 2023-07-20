using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Organization.Queries;
using GHPCommerce.Core.Shared.Events.Guest;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Events;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.DTOs;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Helpers;
using GHPCommerce.Modules.Sales.Repositories;

namespace GHPCommerce.Modules.Sales.Events
{
    public class OrdersEventHandler :IEventHandler<GuestPickupCreatedEvent>,
        IEventHandler<GuestShipCreatedEvent>
    {
        private readonly IOrdersRepository _ordersRepository;
        private readonly ICommandBus _commandBus;
        private readonly IEmailSender _emailSender;

        public OrdersEventHandler(IOrdersRepository ordersRepository, ICommandBus commandBus, IEmailSender emailSender)
        {
            _ordersRepository = ordersRepository;
            _commandBus = commandBus;
            _emailSender = emailSender;
        }
        public async Task Handle(GuestPickupCreatedEvent notification, CancellationToken cancellationToken)
        {
            var orderTemplates = new List<OrderEmailDto>();
            var organizationQuery = await _commandBus.SendAsync(new GetPharmacistQuery(), cancellationToken);
            var currentVendor = organizationQuery.FirstOrDefault(x => x.Id == notification.VendorId);
            foreach (var groupedItems in notification.ShoppingCartItemModels.GroupBy(x=>x.OrganizationId))
            {
                var order = new Order();
                if (groupedItems.Key != null) order.SupplierId = groupedItems.Key.Value;
                order.CustomerId = notification.VendorId;
                order.GuestId = notification.GuestId;
                var wholeSaler =await _commandBus.SendAsync(new GetWholesaleByIdQueryV2 { SupplierOrganizationId = order.SupplierId}, cancellationToken);
                order.SupplierName = wholeSaler?.Name;
                order.CustomerName = notification.CustomerName;
                order.OrderDate = DateTime.Now.Date;
                foreach (var shoppingCartItem in groupedItems)
                {
                    var orderItem = new OrderItem();
                    orderItem.ProductId = shoppingCartItem.ProductId;
                    orderItem.Quantity = shoppingCartItem.Quantity;
                    orderItem.Discount = shoppingCartItem.Discount;
                    orderItem.Tax = shoppingCartItem.Tax;
                    orderItem.UnitPrice = shoppingCartItem.UnitPrice;
                    orderItem.Color = shoppingCartItem.Color;
                    orderItem.Size = shoppingCartItem.Size;
                    orderItem.ExpiryDate = shoppingCartItem.ExpiryDate;
                    orderItem.ProductName = shoppingCartItem.FullName;
                    orderItem.ProductCode = shoppingCartItem.Code;
                    order.OrderItems.Add(orderItem);
                }
               
                _ordersRepository.Add(order);
                await _ordersRepository.UnitOfWork.SaveChangesAsync();
                orderTemplates.Add(new OrderEmailDto
                {
                    Vendor = notification.CustomerName,
                    Number = order.OrderNumber,
                    Date = DateTime.Now.Date.ToShortDateString(),
                    Guest = notification.Guest,
                    Email = wholeSaler?.Email,
                    Address = currentVendor?.Address.Street ,
                    City = currentVendor?.Address.City,
                    State = currentVendor?.Address.State,
                    Zip = currentVendor?.Address.ZipCode

                });
            }
           
            foreach (var orderTemplate in orderTemplates)
            {
                await _emailSender.SendEmailAsync(notification.Email, "Order Creation",
                    OrderEmailTemplateHelper.GetOrderGuestTemplate(orderTemplate));
            }
           
        }

        public async Task Handle(GuestShipCreatedEvent notification, CancellationToken cancellationToken)
        {
            var orderTemplates = new List<OrderEmailDto>();
            foreach (var groupedItems in notification.ShoppingCartItemModels.GroupBy(x => x.OrganizationId))
            {
                var order = new Order();
                if (groupedItems.Key != null) order.SupplierId = groupedItems.Key.Value;
                
                order.GuestId = notification.GuestId;
                var wholeSaler = await _commandBus.SendAsync(new GetWholesaleByIdQueryV2 { SupplierOrganizationId = order.SupplierId }, cancellationToken);
                order.SupplierName = wholeSaler?.Name;
                order.OrderDate = DateTime.Now.Date;
                order.CustomerName = notification.Guest;
                foreach (var shoppingCartItem in groupedItems)
                {
                    var orderItem = new OrderItem();
                    orderItem.ProductId = shoppingCartItem.ProductId;
                    orderItem.Quantity = shoppingCartItem.Quantity;
                    orderItem.Discount = shoppingCartItem.Discount;
                    orderItem.Tax = shoppingCartItem.Tax;
                    orderItem.UnitPrice = shoppingCartItem.UnitPrice;
                    orderItem.Color = shoppingCartItem.Color;
                    orderItem.Size = shoppingCartItem.Size;
                    orderItem.ExpiryDate = shoppingCartItem.ExpiryDate;
                    orderItem.ProductName = shoppingCartItem.FullName;
                    orderItem.ProductCode = shoppingCartItem.Code;
                    order.OrderItems.Add(orderItem);
                }

                _ordersRepository.Add(order);
                await _ordersRepository.UnitOfWork.SaveChangesAsync();
                orderTemplates.Add(new OrderEmailDto
                {
                    Vendor = wholeSaler?.Name,
                    Number = order.OrderNumber,
                    Date = DateTime.Now.Date.ToShortDateString(),
                    Guest = notification.Guest,
                    Email = wholeSaler?.Email,
                    Address = notification.Address.Street,
                    City = notification.Address.City,
                    State = notification.Address.State,
                    Zip = notification.Address.ZipCode

                });
            }

            foreach (var orderTemplate in orderTemplates)
            {
                await _emailSender.SendEmailAsync(notification.Email, "Order Creation",
                    OrderEmailTemplateHelper.GetOrderGuestTemplate(orderTemplate));
            }
        }
    }
}
