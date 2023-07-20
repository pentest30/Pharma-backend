using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Common.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Guest;
using GHPCommerce.Core.Shared.Events.Guest;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;

namespace GHPCommerce.Application.Tiers.Guests.Commands
{
    public class GuestsCommandsHandler : ICommandHandler<CreateGuestPickupCommand, ValidationResult>,
        ICommandHandler<CreateGuestShipCommand, ValidationResult>
    {
        private readonly IRepository<Guest, Guid> _guestRepository;
        private readonly ICommandBus _commandBus;
        private readonly IMapper _mapper;
        private readonly IMqttClient _mqttClient;

        public GuestsCommandsHandler(IRepository<Guest, Guid> guestRepository,  ICommandBus commandBus, IMapper mapper)
        {
            _guestRepository = guestRepository;
            _commandBus = commandBus;
            _mapper = mapper;
            var options = new MqttClientOptionsBuilder()
                .WithClientId("Client1")
                .WithTcpServer("localhost")
                .Build();
            var factory = new MqttFactory();
            
            _mqttClient = factory.CreateMqttClient();
            _mqttClient.ConnectAsync(options, CancellationToken.None).GetAwaiter().GetResult();
        }
        public async Task<ValidationResult> Handle(CreateGuestPickupCommand request, CancellationToken cancellationToken)
        {
            var existingGuest =await 
                _guestRepository.Table.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
            if (existingGuest == null)
            {
                existingGuest = new Guest();
                existingGuest.Email = request.Email;
                existingGuest.PhoneNumber = request.PhoneNumber;
                existingGuest.CompanyName = request.CompanyName;
                existingGuest.FirstName = request.FirstName;
                existingGuest.LastName = request.LastName;
                _guestRepository.Add(existingGuest);
                await _guestRepository.UnitOfWork.SaveChangesAsync();
            }
            var @event = new GuestPickupCreatedEvent {Guest = $"{request.FirstName} {request.LastName}", Email = request.Email,CustomerName = request.CustomerName,GuestId = existingGuest.Id, ShoppingCartItemModels = request.ShoppingCartItems, VendorId = request.VendorId};
            await _commandBus.Publish(@event, cancellationToken);
            await SendNotificationAsync( @event.Guest, @event.ShoppingCartItemModels, cancellationToken);
            return default;
        }

        private async Task SendNotificationAsync( string guest, List<ShoppingCartItemDto> shoppingCartItemModels, CancellationToken cancellationToken)
        {
           
            var messagePayload = new MqttApplicationMessageBuilder()
                .WithTopic("orders/created")
                .WithPayload(JsonConvert.SerializeObject(new OrderCreatedEvent
                    {Guest = guest, ShoppingCartItemModels = shoppingCartItemModels }))
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();
            if (_mqttClient.IsConnected)
                await _mqttClient.PublishAsync(messagePayload);
        }

        public async Task<ValidationResult> Handle(CreateGuestShipCommand request, CancellationToken cancellationToken)
        {
            var existingGuest = await
                _guestRepository.Table
                    .Include(x=>x.Addresses)
                    .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
            if (existingGuest == null)
            {
                existingGuest = new Guest();
                existingGuest.Id = Guid.NewGuid();
                existingGuest.Email = request.Email;
                existingGuest.PhoneNumber = request.PhoneNumber;
                existingGuest.CompanyName = request.CompanyName;
                existingGuest.FirstName = request.FirstName;
                existingGuest.LastName = request.LastName;
                existingGuest.Addresses = new List<Address>();
              
                existingGuest.Addresses.Add(new Address
                {
                    City = request.City,
                    GuestId = existingGuest.Id,
                    State = request.State,
                    Country = request.Country,
                    Main = true,
                    Billing = true,
                    Shipping = true,
                    Township = request.City,
                    Street = request.Street,
                    ZipCode = request.ZipCode
                });
                _guestRepository.Add(existingGuest);
                await _guestRepository.UnitOfWork.SaveChangesAsync();
            }
            else if (existingGuest.Addresses.Count == 0)
            {
                existingGuest.Addresses = new List<Address>();

                existingGuest.Addresses.Add(new Address
                {
                    City = request.City,
                    GuestId = existingGuest.Id,
                    State = request.State,
                    Country = request.Country,
                    Main = true,
                    Billing = true,
                    Shipping = true,
                    Township = request.City,
                    Street = request.Street,
                    ZipCode = request.ZipCode
                });
                await _guestRepository.UnitOfWork.SaveChangesAsync();
            }
            var @event = new GuestShipCreatedEvent {Address = _mapper.Map<AddressDto>(existingGuest.Addresses.FirstOrDefault()),  Guest = $"{request.FirstName} {request.LastName}", Email = request.Email, GuestId = existingGuest.Id, ShoppingCartItemModels = request.ShoppingCartItems };

            await _commandBus.Publish(@event, cancellationToken);
            await SendNotificationAsync(@event.Guest, @event.ShoppingCartItemModels, cancellationToken);

            return default;
        }

       
    }
}
