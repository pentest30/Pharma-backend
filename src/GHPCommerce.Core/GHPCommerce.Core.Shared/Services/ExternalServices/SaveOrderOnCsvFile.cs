using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Orders;
using GHPCommerce.Core.Shared.Contracts.Orders.Dtos;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Domain.Domain.Identity;

namespace GHPCommerce.Core.Shared.Services.ExternalServices
{
    public class SaveOrderOnCsvFile : ISaveOrderOnExternalService<ValidationResult>
    {
        private readonly CustomerDtoV1 _customer;
        private readonly OrderDtoV3 _order;
        private readonly User _currentUser;

        public SaveOrderOnCsvFile(CustomerDtoV1 customer, OrderDtoV3 order, User currentUser)
        {
            _customer = customer;
            _order = order;
            _currentUser = currentUser;
        }

        public async Task<ValidationResult> SaveAsync()
        {
            var csv = new StringBuilder();
            var orderHeader =
                "code client;Reference commande client;cree par;Date et heure de creation de la commande;Session commercial;Session superviseur;Date de livraison;Date echeance;Type de commande;Commande speciale;Commande globale;secteur de livraison;Montant HT;Montant Remise;Valeur Remisee;Reference Bon de commande client;Commande a respecte";
            var shippingDate = _order.ExpectedShippingDate ?? DateTime.Now;
            csv.AppendLine(orderHeader);
            var salesManager = !string.IsNullOrEmpty(_customer?.SalesManager) ? _customer.SalesManager : "";
            var sector = !string.IsNullOrEmpty(_customer?.Sector) ? _customer.Sector : "";
            var code = !string.IsNullOrEmpty(_customer?.Code) ? _customer.Code : "";
            var line = code
                       + ";" + _order.OrderNumberSequence
                       + ";" + _order.CreatedBy
                       + ";" + _order.CreatedDateTime.ToString("g")
                       + ";" + _currentUser.UserName
                       + ";" + salesManager
                       + ";" + shippingDate.Date.ToShortDateString()
                       + ";" + _order.CreatedDateTime.AddDays(_customer?.DeadLine ?? 0).Date.ToShortDateString()
                       + ";" + (int)_order.OrderType
                       + ";" + false
                       + ";" + true
                       + ";" + sector
                       + ";" + _order.OrderItems.Sum(x => x.UnitPrice * x.Quantity)
                       + ";" + _order.OrderTotal
                       + ";" + _order.OrderItems.Sum(x =>
                           x.UnitPrice * x.Quantity * (decimal)x.Discount * (decimal)x.ExtraDiscount / 100)
                       + ";" + _order.RefDocument
                       + ";" + _order.ToBeRespected;
            csv.AppendLine(line);
            var orderItemHeader =
                "code article;Numero lot;Quantite;Prix de vente;Remise ligne;Montant HT rz;Conventionné oui/Non;Groupe de taxe client;Groupe de taxe article;Remise mannuelle;";
            csv.AppendLine(orderItemHeader);

            foreach (var orderOrderItem in _order.OrderItems)
            {
                var discount = orderOrderItem.UnitPrice * orderOrderItem.Quantity *
                               ((decimal)orderOrderItem.Discount + (decimal)orderOrderItem.ExtraDiscount);


                var taxCode = "";
                var orderItemLine = orderOrderItem.ProductCode + ";"
                                                               + orderOrderItem.InternalBatchNumber + ";"
                                                               + orderOrderItem.Quantity + ";"
                                                               + orderOrderItem.UnitPrice + ";"
                                                               + orderOrderItem.Discount + ";"
                                                               + Math.Round(discount, 4) + ";"
                                                               + false + ";"
                                                               + taxCode + ";"
                                                               + ";" + orderOrderItem.ExtraDiscount;
                csv.AppendLine(orderItemLine);

            }

            await File.WriteAllTextAsync(
                @"c:\inetpub\orders\order-" + _customer?.Code + "-" +
                _order.CreatedDateTime.ToString("yyyyMMddHHmmss") + ".csv",
                csv.ToString());
            return default;
        }

    }
}