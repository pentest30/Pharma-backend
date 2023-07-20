using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Invoices.DTOs;
using GHPCommerce.Core.Shared.Contracts.Invoices.Queries;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Modules.Procurement.Queries.Invoices
{
    public class GetInvoiceByCustomersQueryHandler :  
        ICommandHandler<GetInvoiceByCustomersQuery, InvoiceDtoV3>
    {
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;
        private readonly IRepository<Invoice, Guid> _repository;
        private readonly IRepository<Order, Guid> _orderRepository;
        private readonly IRepository<User, Guid> _userRepository;
        private readonly ICommandBus _commandBus;
        private readonly SalesDbContext salesDbContext;
        private readonly ICurrentOrganization _currentOrganization;
        public GetInvoiceByCustomersQueryHandler(IMapper mapper,
        ICurrentUser currentUser,
        ICommandBus commandBus,
        IRepository<Invoice, Guid> repository, IRepository<Order, Guid> orderRepository, IRepository<User, Guid> userRepository, SalesDbContext salesDbContext, ICurrentOrganization currentOrganization)
        {
            _mapper = mapper;
            _currentUser = currentUser;
            _repository = repository;
            _commandBus = commandBus;
            _orderRepository = orderRepository;
            _userRepository = userRepository;
            this.salesDbContext = salesDbContext;
            _currentOrganization = currentOrganization;
        }
        public async Task<InvoiceDtoV3> Handle(GetInvoiceByCustomersQuery request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId==null ||orgId.Value==Guid.Empty)
                return null;
            var currentUser =
              await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                  cancellationToken);
            var ids = new List<Guid> { _currentUser.UserId };

            if (currentUser.UserRoles.Any(x => x.Role.Name == "Supervisor"))
            {
                var salesPersonIds = await _commandBus.SendAsync(new GetSalesPersonIdsBySalesManageQuery
                { OrganizationId = orgId.Value, UserId = _currentUser.UserId }, cancellationToken);
                var personIds = salesPersonIds as Guid[] ?? salesPersonIds.ToArray();
                if (personIds.Any())
                    ids.AddRange(personIds);

                var invoiceMonth = (await _repository.Table
                    .Include(inv => inv.InvoiceItems)
                    .AsNoTracking()
                    .Where(inv =>
                     (request.CustomerId==null || inv.CustomerId == request.CustomerId) &&
                    inv.InvoiceDate.Month == request.Date.Month && inv.InvoiceDate.Year == request.Date.Year
                       
                    ).ToListAsync(cancellationToken))
                    .Where(inv=>
                    ids.Any(c => c == inv.SalesPersonId)).ToList();

                if (invoiceMonth == null || invoiceMonth.Count == 0)
                {
                    return new InvoiceDtoV3
                    {
                        OrderTotal = 0,
                        OrderTotalMonth = 0,
                        OrderTotalMonthBenefit = 0,
                        OrderTotalMonthBenefitRate = 0,
                        OrdersPerMonth = 0,
                        OrderTotalMonthPurchasePrice = 0
                    };
                }
                var sumPurchase = (decimal)invoiceMonth
                    .Sum(x => x.InvoiceItems
                    .Sum(i => i.PurchaseDiscountUnitPrice * i.Quantity));

                var invoices =
                     _repository.Table
                    .AsNoTracking()
                    .Where(inv =>
                    (request.CustomerId == null || inv.CustomerId == request.CustomerId) &&
                    inv.CreatedDateTime.Date == request.Date.Date
                    && ids.Any(c => c == inv.SalesPersonId)
                    ); 
                if (invoices != null)
                {
                    var benefitMonth = invoiceMonth.Sum(x => x.Benefit);
                    var benefitDay = await invoices.SumAsync(x => x.Benefit, cancellationToken);
                    var totalMonth = invoiceMonth.Sum(x => x.TotalHT - x.TotalDiscount);
                    var totalDaily = await invoices.SumAsync(x => x.TotalHT - x.TotalDiscount, cancellationToken);
                    return new InvoiceDtoV3
                    {
                        OrderTotal = totalDaily,
                        OrderTotalMonth = totalMonth,
                        OrderTotalMonthBenefit = benefitMonth,
                        OrderTotalMonthBenefitRate = sumPurchase == 0 ? 0 : benefitMonth / sumPurchase,
                        OrdersPerMonth = invoiceMonth.Count,
                        OrderTotalMonthPurchasePrice = sumPurchase,
                        DailyMarkUpRate = totalDaily == 0 ? 0 : benefitDay / totalDaily

                    };
                }
                else return new InvoiceDtoV3();

            }
            else
            if (currentUser.UserRoles.Any(x => x.Role.Name == "SalesPerson"))
            {

                var invoiceMonth = await _repository.Table
                    .Include(inv=>inv.InvoiceItems)
                    .AsNoTracking()
                    .Where(inv =>
                    (request.CustomerId == null || inv.CustomerId == request.CustomerId) && 
                    inv.InvoiceDate.Month == request.Date.Month && inv.InvoiceDate.Year == request.Date.Year
                    && inv.SalesPersonId == _currentUser.UserId
                    ).ToListAsync(cancellationToken);
                var test = invoiceMonth.FindIndex(
    i => i.TotalHT > 0);
                if (test >= 0)
                    ;
                if (invoiceMonth == null || invoiceMonth.Count == 0)
                {
                    return new InvoiceDtoV3
                    {
                        OrderTotal = 0,
                        OrderTotalMonth = 0,
                        OrderTotalMonthBenefit = 0,
                        OrderTotalMonthBenefitRate = 0,
                        OrdersPerMonth = 0,
                        OrderTotalMonthPurchasePrice = 0
                    };
                }
                var sumPurchase = (decimal)invoiceMonth
                    .Sum(x => x.InvoiceItems
                    .Sum(i => i.PurchaseDiscountUnitPrice * i.Quantity));

                var invoices =
                           _repository.Table
                           .Include(inv=>inv.InvoiceItems)
                    .AsNoTracking()
                    .Where(inv => (request.CustomerId == null || inv.CustomerId == request.CustomerId) &&
                    inv.InvoiceDate.Date == request.Date.Date
                    && inv.SalesPersonId == _currentUser.UserId
                    );

                if (invoices != null)
                {
                    var benefitMonth = invoiceMonth.Sum(x => x.Benefit);
                    var benefitDay = await invoices.SumAsync(x => x.Benefit);
                    var totalMonth = invoiceMonth.Sum(x => x.TotalHT - x.TotalDiscount);
                    var totalDaily = await invoices.SumAsync(x => x.TotalHT - x.TotalDiscount);
                    return new InvoiceDtoV3
                    {
                        OrderTotal = totalDaily,
                        OrderTotalMonth = totalMonth,
                        OrderTotalMonthBenefit = benefitMonth,
                        OrderTotalMonthBenefitRate = sumPurchase == 0 ? 0 : benefitMonth / sumPurchase,
                        OrdersPerMonth = invoiceMonth.Count,
                        OrderTotalMonthPurchasePrice = sumPurchase,
                        DailyMarkUpRate = totalDaily == 0 ? 0 : benefitDay / totalDaily

                    };
                }
                else return new InvoiceDtoV3();
            }



            else return new InvoiceDtoV3();
        }
    }
}
