using System;
using AutoMapper;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Domain.Tiers;
using GHPCommerce.Domain.Interfaces;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Inventory.Repositories;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GHPCommerce.Modules.Sales.Test
{
    public class TestBase : IClassFixture<ServiceFixture>
    {
        protected IOrdersRepository OrderRepository;
        protected IMapper Mapper;
        protected ICurrentOrganization CurrentOrg;
        protected ICommandBus CommandBus;
        protected ICurrentUser CurrentUser;
        protected ICache InMemoryCache;
        protected ILockProvider<string> LockProvider;
        protected IHttpContextAccessor HttpContextAccessor;
        protected IRepository<Organization, Guid> OrgRepository;
        protected IInventoryRepository InventoryRepository;
        protected UserManager<User> UserManager;
        public TestBase(ServiceFixture fixture)
        {
            var serviceProvider = fixture.ServiceProvider;
            HttpContextAccessor = serviceProvider.GetService<IHttpContextAccessor>();
            CurrentUser = serviceProvider.GetService<ICurrentUser>();
            Mapper = serviceProvider.GetService<IMapper>();
            CommandBus = serviceProvider.GetService<ICommandBus>();
            CurrentOrg = serviceProvider.GetService<ICurrentOrganization>();
            InMemoryCache = serviceProvider.GetService<ICache>();
            LockProvider = serviceProvider.GetService<ILockProvider<string>>();
            OrgRepository = serviceProvider.GetService<IRepository<Organization, Guid>>();
           // OrderRepository = serviceProvider.GetService<IOrdersRepository>();
            //InventoryRepository = serviceProvider.GetService<IInventoryRepository>();
            UserManager = serviceProvider.GetService<UserManager<User>>();

        }

        
    }
}
