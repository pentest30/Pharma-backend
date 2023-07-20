using System;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Core.Shared.Services.ExternalServices
{
    public static class  ServiceAX2012Factory
    {
        public static SaveOrderOnAx2012 Create(string customerCode, string createdBy, double balance,string user, string pass)
        {
            return new SaveOrderOnAx2012(customerCode,createdBy, balance, user,pass);
        }
        public static SaveOrderOnAx2012 Create(PreparationOrderDtoV6 preparationOrder, OrderDtoV5 order,
            CustomerDtoV1 customer, Guid? pickingZoneId, string pickingZoneName, string user, string pass)
        {
            return new SaveOrderOnAx2012(preparationOrder,order, customer, pickingZoneId.Value, pickingZoneName, user, pass);
        }
    }

    public static class ServiceFileFactory
    {
        public static SaveOrderOnCsvFile Create(CustomerDtoV1 customer, OrderDtoV3 order, User currentUser)
        {
            return new SaveOrderOnCsvFile(customer,order, currentUser);
        }
    }
}