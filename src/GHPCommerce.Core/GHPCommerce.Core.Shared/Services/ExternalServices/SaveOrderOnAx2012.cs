using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Orders;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.PreparationOrders.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Events.PreparationOrder;
using GHPCommerce.Core.Shared.PreparationOrder.DTOs;
using ServiceReference1;

namespace GHPCommerce.Core.Shared.Services.ExternalServices
{
    public class SaveOrderOnAx2012 : ISaveOrderOnExternalService<AxValidationDto>, IValidateOrderOnExternalService
    {
        private readonly PreparationOrderDtoV6 _preparationOrder;
        private readonly OrderDtoV5 _order;
        private readonly CustomerDtoV1 _customer;
        private readonly Guid _pickingZoneId;
        private readonly string _pickingZoneName;

        private readonly string _customerCode;
        private readonly string _createdBy;
        private readonly double _balance;
        private DOSI_SalesOrderServiceClient _client;
        private CallContext _callContext;

        public SaveOrderOnAx2012(string customerCode, string createdBy, double balance, string user, string pass)
        {
            _customerCode = customerCode;
            _createdBy = createdBy;
            _balance = balance;
            InitWcfService(user, pass);
        }

        public SaveOrderOnAx2012(PreparationOrderDtoV6 preparationOrder, OrderDtoV5 order, CustomerDtoV1 customer,Guid pickingZoneId,string pickingZoneName, string user, string pass)
        {
            _preparationOrder = preparationOrder;
            _order = order;
            _customer = customer;
            _pickingZoneId = pickingZoneId;
            _pickingZoneName = pickingZoneName;
            InitWcfService(user, pass);
        }

        private void InitWcfService(string user, string pass)
        {
            _client = new DOSI_SalesOrderServiceClient();
            _callContext = new CallContext();
            _callContext.Company = "HP";
            _client.ClientCredentials.Windows.ClientCredential.UserName = user;
            _client.ClientCredentials.Windows.ClientCredential.Password = pass;
        }

        public async Task ValidateAsync()
        {
            //Vérifier l'entete de la commande
            var checkOrderResponse = await _client.checkOrderAsync(_callContext, _customerCode, _createdBy, _balance);
            var msg = checkOrderResponse.response.comments;
            if (msg.Any())
                RaiseValidationException(msg.Select(x => x.Value).ToList());

        }

        public async Task<AxValidationDto> SaveAsync()
        {
            var salesOrder = GetSalesOrder();
            var msg = await _client.createSOfromPOAsync(_callContext, salesOrder);
            var validationErrors = ParseAxResponse(msg);
            return validationErrors;
        }

        private DOSI_SalesOrderDC GetSalesOrder()
        {
            DOSI_SalesOrderDC salesOrder = new DOSI_SalesOrderDC();
            List<DOSI_SalesLineDC> salesLineArray = new List<DOSI_SalesLineDC>();
            salesOrder.customerId = _customer.Code;
            salesOrder.codeCmdGHPC = _order.SequenceNumber;
            salesOrder.orderType = _order.OrderType + 1;
            salesOrder.createdBy = _order.CreatedBy;
            salesOrder.zone = _pickingZoneName;
            if (!string.IsNullOrEmpty(_order.RefDocument) && !string.IsNullOrWhiteSpace(_order.RefDocument))
            {
                salesOrder.CustomerRef = _order.RefDocument;
            }

            foreach (var orderOrderItem in _preparationOrder.PreparationOrderItems.Where(c => c.Status != BlStatus.Deleted && c.PickingZoneId == _pickingZoneId))
            {
                var itemSale = new DOSI_SalesLineDC();
                itemSale.qty = orderOrderItem.Quantity;
                itemSale.itemId = orderOrderItem.ProductCode;
                itemSale.batchId = orderOrderItem.InternalBatchNumber;
                if (orderOrderItem.Discount > 0) itemSale.multiLnPercent = orderOrderItem.Discount * 100;
                if (orderOrderItem.ExtraDiscount > 0) itemSale.linePercent = orderOrderItem.ExtraDiscount * 100;
                salesLineArray.Add(itemSale);
            }

            salesOrder.SalesLine = salesLineArray.ToArray();
            return salesOrder;
        }

        private static AxValidationDto ParseAxResponse(DOSI_SalesOrderServiceCreateSOfromPOResponse msg)
        {
            var validationErrors = new AxValidationDto();
            if (!msg.response.POCreated)
            {
                validationErrors.IsValid = false;
                if (msg.response.headerComments != null && msg.response.headerComments.Any())
                {
                    foreach (var responseHeaderComment in msg.response.headerComments)
                    {
                        validationErrors.OrderValidationErrors.Add(responseHeaderComment.Key.ToString(),
                            responseHeaderComment.Value);
                    }
                }

                if (msg.response.POLines != null && msg.response.POLines.Any())
                {
                    foreach (var item in msg.response.POLines)
                    {
                        var orderItemValidationErrors = item.lineComments.ToDictionary(responseHeaderComment => responseHeaderComment.Key.ToString(), responseHeaderComment => responseHeaderComment.Value);
                        validationErrors.InvalidItems.Add(new InvalidPreparationOderItemDto
                        {
                            InternalBatchNumber = item.batchId,
                            ProductCode = item.itemId,
                            Quantity = item.qty,
                            OrderItemValidationErrors = orderItemValidationErrors
                        });
                    }
                }
            }
            else
            {
                validationErrors.IsValid = true;
                validationErrors.CodeAx = msg.response.salesId;
            }

            return validationErrors;
        }
        private static void RaiseValidationException(List<string> r)
        {
            if (!r.Any()) return;
            var exceptions = new List<Exception>();
            foreach (var msg in r)
            {
                exceptions.Add(new InvalidOperationException(msg));
            }

            throw new AggregateException("Erreurs de validation côté AX", exceptions);
        }
    }
}