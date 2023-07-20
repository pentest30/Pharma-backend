using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Batches.Commands;
using GHPCommerce.Core.Shared.Contracts.Batches.Queries;
using GHPCommerce.Core.Shared.Contracts.Inventory.Commands;
using GHPCommerce.Core.Shared.Contracts.Inventory.Queries;
using GHPCommerce.Core.Shared.Contracts.Invoices;
using GHPCommerce.Core.Shared.Contracts.Transactions;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Commands.Invoices
{
    public class ValidateInvoiceCommandHandler: ICommandHandler<ValidateInvoiceCommand, ValidationResult>
     {
         private readonly IRepository<SupplierInvoice, Guid> _repository;
         private readonly IRepository<SupplierOrder, Guid> _orderRepository;
         private readonly ICommandBus _commandBus;
         private readonly IMapper _mapper;
         private readonly ICurrentOrganization _currentOrganization;
         private List<Guid> _batchIds = new List<Guid>();

         public ValidateInvoiceCommandHandler(IRepository<SupplierInvoice, Guid> repository, 
             IRepository<SupplierOrder, Guid> orderRepository, 
             ICommandBus commandBus,
             IMapper mapper,
             ICurrentOrganization currentOrganization)
         {
             _repository = repository;
             _orderRepository = orderRepository;
             _commandBus = commandBus;
             _mapper = mapper;
             _currentOrganization = currentOrganization;
         }

         public async Task<ValidationResult> Handle(ValidateInvoiceCommand request, CancellationToken cancellationToken)
         {
             try
             {
                 var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
                 if (orgId == null)
                     throw new InvalidOperationException($"resources not allowed");

                 var invoice = await _repository
                     .Table
                     .Include(x => x.Items)
                     .FirstOrDefaultAsync(x => x.Id == request.InvoiceId, cancellationToken: cancellationToken);
                 if (invoice == null)
                     throw new NotFoundException($"invoice with id {request.InvoiceId} was not found");
                 var order = await _orderRepository.Table
                     .Include(x => x.OrderItems)
                     .FirstOrDefaultAsync(x => x.Id == invoice.OrderId, cancellationToken: cancellationToken);
                 //order.i
                 foreach (var invoiceItem in invoice.Items)
                 {
                     var item = order.OrderItems.FirstOrDefault(x => x.ProductId == invoiceItem.ProductId);
                     if (item != null)
                     {
                         item.InvoicedQuantity += invoiceItem.Quantity;
                         if (item.RemainingQuantity < invoiceItem.Quantity)
                             item.RemainingQuantity = 0;
                         else
                             item.RemainingQuantity -= invoiceItem.Quantity;
                         item.WaitForDelivery = true;
                     }

                     invoiceItem.RemainingQuantity = invoiceItem.Quantity;
                     invoiceItem.InvoicedQuantity = invoiceItem.Quantity;

                     await CreateNewBatch(invoice, invoiceItem, cancellationToken);
                 }

                 if (order.OrderItems.Sum(x => x.RemainingQuantity) == 0)
                     order.OrderStatus = ProcurmentOrderStatus.Completed;

                 invoice.InvoiceStatus = InvoiceStatus.InProgress;
                 _repository.Update(invoice);
                 _orderRepository.Update(order);
                 await _repository.UnitOfWork.SaveChangesAsync();
                 return default;
             }
             catch (Exception ex)
             {
                 var validations = new ValidationResult
                     { Errors = { new ValidationFailure("Transaction rolled back", ex.Message) } };
                // in case when the validation of invoice fails
                 foreach (var batchId in _batchIds)
                     await _commandBus.SendAsync(new DeleteBatchCommand { BatchId = batchId }, cancellationToken);
                 return validations;
             }
         }

         private async Task CreateNewBatch( SupplierInvoice invoice ,SupplierInvoiceItem invoiceItem, CancellationToken cancellationToken)
         {
             var batchId = await _commandBus.SendAsync(new GetBatchIdQuery
             {
                 InternalBatchNumber = invoiceItem.InternalBatchNumber,
                 VendorBatchNumber = invoiceItem.VendorBatchNumber,
                 ProductId = invoiceItem.ProductId
             }, cancellationToken);
             if (batchId == Guid.Empty)
             {
                 var batch = _mapper.Map<CreateBatchCommand>(invoiceItem);
                 batch.RefDoc = invoice.InvoiceSequenceNumber;
                 batch.OrganizationId = invoice.CustomerId;
                 batch.OrderId = invoice.OrderId;
                 batch.OrganizationName = invoice.CustomerName;
                 batch.SupplierId = invoice.SupplierId;
                 batch.SupplierName = invoice.SupplierName;
                 var result = await _commandBus.SendAsync(batch, cancellationToken);
                 if (result.Item2.IsValid)
                     batchId = result.Item1;
                 else
                     ReturnValidationErrors(result.Item2);

             }
             _batchIds.Add( batchId);
             var originQuantity = await _commandBus.SendAsync(new GetInventQuantityQuery
             {
                 ProductId = invoiceItem.ProductId,
                 InternalBatchNumber = invoiceItem.InternalBatchNumber ,
                 VendorBatchNumber = invoiceItem.VendorBatchNumber
             }, cancellationToken);
             var inventResult =
                 await CreateOrUpdateInvent(batchId, invoice, invoiceItem, cancellationToken);
             if (inventResult.Item2.IsValid)
             {
                 var r = await GenerateInvoiceTransaction(invoice, invoiceItem, inventResult, originQuantity, cancellationToken);
                 if(r!=null&& !r.IsValid)
                     ReturnValidationErrors(r);
             }
             else ReturnValidationErrors(inventResult.Item2);
         }

         private static void ReturnValidationErrors(ValidationResult result)
         {
             var m = "";
             foreach (var validationFailure in result.Errors)
                 m = validationFailure.ErrorMessage + "\n";
             throw new InvalidOperationException(m);
         }

         private async Task<ValidationResult> GenerateInvoiceTransaction(SupplierInvoice invoice, SupplierInvoiceItem invoiceItem, Tuple<Guid, ValidationResult> inventResult, 
             double originQuantity,
             CancellationToken cancellationToken)
         {
             var trans = _mapper.Map<CreateAtSupplierInventTransactionCommand>(invoiceItem);
             trans.InventId = inventResult.Item1;
             trans.RefDoc = invoice.InvoiceSequenceNumber;
             trans.CustomerId = invoice.CustomerId;
             trans.CustomerName = invoice.CustomerName;
             trans.OrderId = invoice.OrderId;
             trans.OrganizationName = invoice.CustomerName;
             trans.SupplierId = invoice.SupplierId;
             trans.SupplierName = invoice.SupplierName;
             trans.OrderDate = invoice.InvoiceDate;
             trans.NewQuantity = 0;
             trans.OriginQuantity = originQuantity;
             
             var r = await _commandBus.SendAsync(trans, cancellationToken);
             return r;
         }

         private async Task<Tuple<Guid, ValidationResult>> CreateOrUpdateInvent(Guid batchId,  SupplierInvoice invoice , SupplierInvoiceItem invoiceItem, CancellationToken cancellationToken)
         {
             var invent = _mapper.Map<CreateOrUpdateInventCommand>(invoiceItem);
             invent.BatchId = batchId;
             invent.OrganizationId = invoice.CustomerId;
             invent.OrganizationName = invoice.CustomerName;
             invent.SupplierId = invoice.SupplierId;
             invent.SupplierName = invoice.SupplierName;
             invent.ZoneId = Guid.Parse("7BD42E23-E657-4F99-AFEF-1AFE5CEACB16");
             invent.ZoneName = "Zone Chez le fournisseur";
             invent.StockStateId = Guid.Parse("7BD13E23-E657-4F99-AFEF-1AFE5CEACB16");
             invent.StockStateName = "RAL";
             var inventResult = await _commandBus.SendAsync(invent, cancellationToken);
             return inventResult;
         }
     }
}