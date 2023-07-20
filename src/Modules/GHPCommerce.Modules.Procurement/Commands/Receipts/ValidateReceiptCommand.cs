using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.Procurement.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Commands.Receipts
{
    public class ValidateReceiptCommand :SaveReceiptCommand
    {
    }
    public class ValidateReceiptCommandHandler : ICommandHandler<ValidateReceiptCommand, ValidationResult>
    {
        private readonly IRepository<DeliveryReceipt, Guid> _repository;
        private readonly IRepository<SupplierInvoice, Guid> _invoiceRepository;
        private readonly IPublishEndpoint _bus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IMapper _mapper;
        private readonly ICurrentUser _currentUser;

        public ValidateReceiptCommandHandler(IRepository<DeliveryReceipt, Guid> repository, 
            IRepository<SupplierInvoice, Guid> invoiceRepository,
            IPublishEndpoint bus, 
            ICurrentOrganization currentOrganization, 
            IMapper mapper,
            ICurrentUser currentUser)
        {
            _repository = repository;
            _invoiceRepository = invoiceRepository;
            _bus = bus;
            _currentOrganization = currentOrganization;
            _mapper = mapper;
            _currentUser = currentUser;
        }
        public async Task<ValidationResult> Handle(ValidateReceiptCommand request, CancellationToken cancellationToken)
        {
            var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (orgId == null) throw new InvalidOperationException("Resources not allowed");

            var receipt = await _repository.Table
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == request.Id
                                          && x.OrganizationId == orgId.Value, cancellationToken: cancellationToken);


            try
            {

                if (receipt == null)
                    throw new NotFoundException($"Receipt with {request.Id} was not found");
                await LockProvider<string>.WaitAsync(receipt.InvoiceNumber + orgId, cancellationToken);
                var invoice = await _invoiceRepository.Table
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.Id == receipt.InvoiceId, cancellationToken: cancellationToken);
                if (invoice == null || invoice.InvoiceStatus == InvoiceStatus.Closed)
                    throw new NotFoundException($"Invoice  was not found or it has been closed");
                foreach (var supplierInvoiceItem in invoice.Items)
                {
                    var receiptItem = receipt.Items.FirstOrDefault(x => x.ProductId == supplierInvoiceItem.ProductId
                                                                        && x.InternalBatchNumber == supplierInvoiceItem.InternalBatchNumber
                                                                        && x.UnitPrice == supplierInvoiceItem.PurchaseUnitPrice);
                    if (receiptItem != null)
                    {
                        supplierInvoiceItem.ReceivedQuantity += receiptItem.Quantity;
                        supplierInvoiceItem.RemainingQuantity -= receiptItem.Quantity;
                    }
                }

                if (invoice.Items.Sum(x => x.RemainingQuantity) == 0)
                    invoice.InvoiceStatus = InvoiceStatus.Closed;
                receipt.Status = InvoiceStatus.Valid;
                _invoiceRepository.Update(invoice);
                await _invoiceRepository.UnitOfWork.SaveChangesAsync();
                _repository.Update(receipt);
                await _repository.UnitOfWork.SaveChangesAsync();
                await _bus.Publish<IDeliveryReceiptSubmittedEvent>(new
                {
                    DeliveryReceiptId = receipt.Id,
                    CorrelationId = Guid.NewGuid(),
                    ItemEvents = _mapper.Map<List<DeliveryItem>>(receipt.Items),
                    OrganizationId = receipt.OrganizationId,
                    Userid = _currentUser.UserId,
                    RefDoc =  "BR-"+ receipt.InvoiceDate.Date.ToString("yy-MM-dd").Substring(0,2)
                                   +"/" +"0000000000".Substring(0,10-receipt.SequenceNumber.ToString().Length)+ receipt.SequenceNumber

                }, cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                var validations = new ValidationResult
                    { Errors = { new ValidationFailure("Transaction rolled back", e.Message) } };
                if (receipt != null)
                    LockProvider<string>.Release(receipt.InvoiceNumber + orgId);
                return validations;

            }
            finally
            {
                if(receipt!=null) 
                    LockProvider<string>.Release(receipt.InvoiceNumber + orgId);
            }
            return default!;
        }
    }
}