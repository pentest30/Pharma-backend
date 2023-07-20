using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Batches.Queries;
using GHPCommerce.Core.Shared.Contracts.DeliveryOrder.Queries;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.SalesInvoice;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Commands.Invoices
{

    public class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, ValidationResult>
    {
        private readonly IRepository<Invoice, Guid> _repository;
        private readonly IRepository<Order, Guid> _ordersRepository;
        private readonly IRepository<FinancialTransaction, Guid> _transactionRepository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly ISequenceNumberService<Invoice, Guid> _sequenceNumberService;
        private SalesDbContext _context;

        public CreateInvoiceCommandHandler(
            IRepository<Invoice, Guid> repository,
            IRepository<FinancialTransaction, Guid> transactionRepository,
            IRepository<Order, Guid> ordersRepository,
            ICurrentOrganization currentOrganization,
            IMapper mapper,
            ICurrentUser currentUser,
            ICommandBus commandBus,
            SalesDbContext context,
            ISequenceNumberService<Invoice, Guid> sequenceNumberService)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _ordersRepository = ordersRepository;
            _transactionRepository = transactionRepository;
            _mapper = mapper;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _sequenceNumberService = sequenceNumberService;
            _context = context;
        }
        public async Task<ValidationResult> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == default) throw new InvalidOperationException("");
            var existInvoice = await _repository.Table.Where(c => c.DeliveryOrderId == request.DeliveryOrderId).FirstOrDefaultAsync();
            if (existInvoice != null)
            {
                var validations = new ValidationResult
                { Errors = { new ValidationFailure("Id", "Invoice related to already order Id already exist") } };

                return validations;
            }
            var deliveryOrder = await _commandBus.SendAsync(new GetDeliveryOrderByIdQuery { Id = request.DeliveryOrderId });
            var order = await _ordersRepository.Table.Where(c => c.Id == deliveryOrder.OrderId).FirstOrDefaultAsync(cancellationToken);
            var customer = await _commandBus.SendAsync(
                new GetCustomerByOrganizationIdQuery { OrganizationId = (Guid)order.CustomerId }, cancellationToken);
            var currentUser =
                await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                    cancellationToken);
            try
            {
                var invoice = new Invoice
                {
                    Region = customer.City,
                    CodeRegion = customer.ZipCode?.Substring(0, 2),
                    Sector = customer.Sector,
                    SectorCode = customer.SectorCode,
                    CreatedBy = currentUser.UserName,
                    CustomerAddress = customer.Street,
                    CustomerCode = customer.Code,
                    CustomerId = order.CustomerId,
                    CustomerName = order.CustomerName,
                    DueDate = DateTime.Now.AddDays(customer.DeadLine),
                    NumberDueDays = customer.DeadLine,
                    InvoiceDate = DateTime.Now,
                    OrganizationId = deliveryOrder.OrganizationId,
                    InvoiceType = order.OrderType,
                    OrderId = order.Id,
                    DeliveryOrderId = request.DeliveryOrderId,
                    OrderDate = order.OrderDate,
                    OrderNumber = order.OrderNumberSequence,
                    SupplierId = order.SupplierId,
                    TotalPackageThermolabile = deliveryOrder.TotalPackageThermolabile,
                    TotalPackage = deliveryOrder.TotalPackage,
                    NumberOfPrints = 0,
                    TotalTax = 0,
                    TotalDiscount = 0,
                    TotalDisplayedDiscount = 0,
                    TotalHT = 0,
                    TotalTTC = 0,
                    SalesPersonId=order.CreatedByUserId

                };
                invoice.InvoiceItems = _mapper.Map<List<InvoiceItem>>(deliveryOrder.DeleiveryOrderItems);
                var i = 0;
                decimal totalBenefit = 0;
                decimal sumPurchase = 0;
                foreach (var item in invoice.InvoiceItems)
                {
                    var deliveryOrderItem = deliveryOrder.DeleiveryOrderItems.Find(c =>
                        c.ProductId == item.ProductId && c.InternalBatchNumber == item.InternalBatchNumber);
                    var batch = await _commandBus.SendAsync(new GetBatchByIdQuery
                    {
                        ProductId = item.ProductId,
                        InternalBatchNumber = item.InternalBatchNumber,
                        VendorBatchNumber = item.VendorBatchNumber
                    }, cancellationToken);
                    if (batch != null)
                    {
                        item.PurchaseDiscountUnitPrice = (decimal)(batch.PurchaseUnitPrice -
                                                                    (batch.PurchaseUnitPrice *
                                                                     batch.PurchaseDiscountRatio));
                        sumPurchase += (decimal)(item.Quantity * batch.PurchaseUnitPrice);
                        totalBenefit += (item.TotalExlTax - item.TotalDiscount) - (decimal)(item.Quantity * batch.PurchaseUnitPrice.Value);
                    }
                    else
                    {
                        item.PurchaseDiscountUnitPrice = 0;
                        sumPurchase += 0;
                        totalBenefit += (item.TotalExlTax - item.TotalDiscount);
                    }
                    item.LineNum = i + 1;
                    item.PpaTTC = item.PpaHT + ((decimal)item.Tax * item.PpaHT);
                    item.TotalDiscount = (decimal)(deliveryOrderItem.Discount + deliveryOrderItem.ExtraDiscount) * item.UnitPrice;
                    item.TotalTax = item.Quantity * ((decimal)item.Tax * item.UnitPrice);
                    item.TotalExlTax = item.Quantity * item.UnitPrice;
                    item.TotalInlTax = ((item.Quantity * item.UnitPrice) - item.TotalDiscount) + item.TotalTax;
                    item.DiscountRate = deliveryOrderItem.Discount + deliveryOrderItem.ExtraDiscount;
                    item.DisplayedDiscountRate = 0.90;
                    item.DisplayedTotalDiscount = (decimal)item.DisplayedDiscountRate * item.TotalInlTax;

                }

                invoice.TotalTax = invoice.InvoiceItems.Sum(c => c.TotalTax);
                invoice.TotalDiscount = invoice.InvoiceItems.Sum(c => c.TotalDiscount);
                invoice.TotalHT = invoice.InvoiceItems.Sum(c => c.TotalExlTax);
                invoice.TotalTTC = invoice.InvoiceItems.Sum(c => c.TotalInlTax);
                invoice.TotalDisplayedDiscount = invoice.InvoiceItems.Sum(c => c.DisplayedTotalDiscount);
                invoice.Benefit = totalBenefit;
                if (sumPurchase > 0)
                    invoice.BenefitRate = totalBenefit / sumPurchase;
                else invoice.BenefitRate = 0;

                var keysq = nameof(Invoice) + orgId;
                await LockProvider<string>.WaitAsync(keysq, cancellationToken);
                var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(invoice.InvoiceDate, orgId.Value);
                invoice.SequenceNumber = sq;
                LockProvider<string>.Release(keysq);
                _repository.Add(invoice);
                await _repository.UnitOfWork.SaveChangesAsync();
                var transaction = new FinancialTransaction
                {
                    DocumentDate = invoice.InvoiceDate,
                    OrganizationId = orgId.Value,
                    RefDocument = "BC-" + invoice.OrderDate.Date.ToString("yy-MM-dd").Substring(0, 2)
                                       + "/" + "0000000000".Substring(0, 10 - invoice.OrderNumber.ToString().Length) + invoice.OrderNumber,
                    FinancialTransactionType = FinancialTransactionType.SalesInvoice,
                    CustomerId = invoice.CustomerId,
                    CustomerName = invoice.CustomerName,
                    SupplierId = invoice.SupplierId,
                    RefNumber = invoice.OrderNumber,
                    TransactionAmount = invoice.TotalTTC
                };
                _transactionRepository.Add(transaction);
                await _transactionRepository.UnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                var validations = new ValidationResult
                { Errors = { new ValidationFailure("Transaction rolled back", e.Message) } };

                return validations;
            }

            return default;

        }
    }
}