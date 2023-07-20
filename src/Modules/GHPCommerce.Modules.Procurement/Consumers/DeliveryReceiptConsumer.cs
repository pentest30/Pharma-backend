using System;
using System.Linq;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Events.DeliveryReceipts;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Procurement.Entities;
using GHPCommerce.Modules.Procurement.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Consumers
{
    public class DeliveryReceiptConsumer : 
        IConsumer<IdeliverReceiptCancelledEvent>,
        IConsumer<DeliveryReceiptCreated>
    {
        private readonly IRepository<DeliveryReceipt, Guid> _deliveryRepository;
        private readonly IRepository<SupplierInvoice, Guid> _invoiceRepository;
        private readonly IHubContext<ProcurementHub> _hubContext;
        private readonly ICache _redisCache;

        public DeliveryReceiptConsumer(IRepository<DeliveryReceipt, Guid> deliveryRepository, 
            IRepository<SupplierInvoice, Guid> invoiceRepository,
            IHubContext<ProcurementHub> hubContext,
            ICache redisCache)
        {
            _deliveryRepository = deliveryRepository;
            _invoiceRepository = invoiceRepository;
            _hubContext = hubContext;
            _redisCache = redisCache;
        }

        public async Task Consume(ConsumeContext<DeliveryReceiptCreated> context)
        {
            var cnxId = _redisCache.Get<string>("proc_hub" + context.Message.UserId);
            await _hubContext.Clients.Client(cnxId).SendAsync("getValidationReceiptNotification" , "Validation terminée avec succès");
        }

        public async Task Consume(ConsumeContext<IdeliverReceiptCancelledEvent> context)
        {
            var receipt = await _deliveryRepository.Table
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == context.Message.DeliveryReceiptId
                                          && x.OrganizationId == context.Message.OrganizationId);
            var invoice = await _invoiceRepository.Table
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == receipt.InvoiceId);
            if (invoice == null || invoice.InvoiceStatus == InvoiceStatus.Closed)
                throw new NotFoundException($"Invoice  was not found or it has been closed");

            receipt.Status = InvoiceStatus.Saved;
            foreach (var supplierInvoiceItem in invoice.Items)
            {
                var receiptItem = receipt.Items
                    .FirstOrDefault(x => x.ProductId == supplierInvoiceItem.ProductId
                                                                    && x.InternalBatchNumber ==supplierInvoiceItem.InternalBatchNumber
                                                                    && x.UnitPrice == supplierInvoiceItem.PurchaseUnitPrice);
                if (receiptItem != null)
                {
                    if (supplierInvoiceItem.ReceivedQuantity >= receiptItem.Quantity)
                        supplierInvoiceItem.ReceivedQuantity -= receiptItem.Quantity;
                    else supplierInvoiceItem.Quantity = 0;
                    supplierInvoiceItem.RemainingQuantity += receiptItem.Quantity;
                }
            }

            invoice.InvoiceStatus = InvoiceStatus.Valid;
            _invoiceRepository.Update(invoice);
            await _invoiceRepository.UnitOfWork.SaveChangesAsync();
            _deliveryRepository.Update(receipt);
            await _deliveryRepository.UnitOfWork.SaveChangesAsync();
            var cnxId = _redisCache.Get<string>("proc_hub" + context.Message.UserId);
            await _hubContext.Clients.Client(cnxId).SendAsync("getValidationReceiptNotification" , "Echec de la Validation ");

        }
    }
}