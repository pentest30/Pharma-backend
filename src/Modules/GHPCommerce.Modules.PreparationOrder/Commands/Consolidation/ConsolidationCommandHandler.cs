using GHPCommerce.Domain.Domain.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using FluentValidation.Results;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Core.Shared.Contracts.Orders.Commands;
using iTextSharp.text;
using iTextSharp.text.pdf;
using GHPCommerce.Infra.OS.Print;
using GHPCommerce.Modules.PreparationOrder.Helpers;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using System.Linq;
using DotNet.Util;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Modules.PreparationOrder.Commands.DeleiveryOrder;
using Microsoft.EntityFrameworkCore;
using ServiceReference1;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using Serilog.Core;

namespace GHPCommerce.Modules.PreparationOrder.Commands.Consolidation
{
    public class ConsolidationCommandHandler : ICommandHandler<ConsolidationCommand, ValidationResult>,
         ICommandHandler<ConsolidationUpdateCommand, ValidationResult>,
         ICommandHandler<PrintConsolidationOrderLabelCommand, ValidationResult>

    {
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly IRepository<ConsolidationOrder, Guid> _repository;
        private readonly IRepository<Entities.PreparationOrder, Guid> _preparationOrderRepository;

        private readonly IRepository<Entities.DeleiveryOrder, Guid> _deliveryOrderRepository;
        private readonly PrinterOptions _printerOptions;
        private readonly MedIJKModel _model;
        private readonly Logger _logger;
        public ConsolidationCommandHandler(
           IMapper mapper,
           ICommandBus commandBus,
           IRepository<ConsolidationOrder, Guid> repository,
           IRepository<Entities.PreparationOrder, Guid> preparationOrderRepository,
           IRepository<Entities.DeleiveryOrder, Guid> deliveryOrderRepository,
           PrinterOptions printerOptions,
           MedIJKModel model, Logger logger)

        {
            _mapper = mapper;
            _commandBus = commandBus;
            _repository = repository;
            _deliveryOrderRepository = deliveryOrderRepository;
            _preparationOrderRepository = preparationOrderRepository;
            _printerOptions = printerOptions;
            _model = model;
            _logger = logger;
        }
        public async Task<ValidationResult> Handle(ConsolidationCommand request, CancellationToken cancellationToken)
        {

            var consolidationOrder = _mapper.Map<ConsolidationOrder>(request);
            consolidationOrder.ReceivedInShippingBy = request.ReceivedInShippingByName;
            consolidationOrder.ReceivedInShippingId = request.ReceivedInShippingById;
            consolidationOrder.ConsolidatedTime = DateTime.Now;
            consolidationOrder.Consolidated = false;
            _repository.Add(consolidationOrder);
            try
            {
                await _repository.UnitOfWork.SaveChangesAsync();
                var result=await _commandBus.SendAsync(new PrintConsolidationOrderLabelCommand { Id = consolidationOrder.Id }, cancellationToken);
                
                if (result != default)
                {
                    if (result.Errors[0].PropertyName == "NB Colis")
                    {
                        var allDeleted = await _preparationOrderRepository.Table.AsNoTracking()
                            .Include(p => p.PreparationOrderItems)
                            .Where(p=>p.OrderId==consolidationOrder.OrderId)
                            .SelectMany(p => p.PreparationOrderItems)
                            .AllAsync(p => p.Status ==BlStatus.Deleted,cancellationToken
                            );
                        if(allDeleted)
                        await _commandBus.SendAsync(new UpdateOrderStatusCommand { Id = consolidationOrder.OrderId, OrderStatus = 70 }, cancellationToken);
                        return result;
                    }
                }
                await _commandBus.SendAsync(new UpdateOrderStatusCommand { Id = consolidationOrder.OrderId, OrderStatus = 100 }, cancellationToken);
                var response = await SendInvoiceToAx(consolidationOrder, cancellationToken);
                if(response) 
                    await _commandBus.SendAsync(new UpdateOrderStatusCommand { Id = consolidationOrder.OrderId, OrderStatus = 140 }, cancellationToken);
            }
            catch (Exception e)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Bl Not Found", e.Message)
                    }
                };
            }
            return default;

        }

        private async Task<bool> SendInvoiceToAx( ConsolidationOrder consolidationOrder, CancellationToken cancellationToken)
        {
            //if (consolidationOrder.TotalPackage + consolidationOrder.TotalPackageThermolabile == 0)
            //    return true;
            var order = await _commandBus.SendAsync(new GetOrderByIdQueryV2 { Id = consolidationOrder.OrderId },cancellationToken);
            // envoyer la commande à la facturation
            if (order != null)
            {
                DOSI_SalesOrderServiceClient client = new DOSI_SalesOrderServiceClient();
                CallContext callContext = new CallContext();
                callContext.Company = "HP";
                client.ClientCredentials.Windows.ClientCredential.UserName = _model.UserAx;
                client.ClientCredentials.Windows.ClientCredential.Password =  _model.PasswordAx; // Code société dans AX
                var msg = await client.invoiceAsync(callContext, order.CodeAx, consolidationOrder.TotalPackage,
                    consolidationOrder.TotalPackageThermolabile);
                // Voir les messages d'erreur
                if (msg.response!.comments != null)
                {
                    var errorMsg = "";
                    foreach (KeyValuePair<int, String> m in msg.response.comments) 
                    {
                        errorMsg += m.Value + "\r";
                    }
                    _logger.Error($"Erreur Facturation AX {order.CodeAx} , {order.SequenceNumber} :{errorMsg}");
                    throw new InvalidOperationException(errorMsg);
                }

                return true;
            }

            return false;
        }

        public async Task<ValidationResult> Handle(ConsolidationUpdateCommand request, CancellationToken cancellationToken)
        {
            var consolidationOrder = await _repository.Table.Where(c => c.Id == request.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            bool totalPackageChanged = (consolidationOrder.TotalPackage != request.TotalPackage)
                || (consolidationOrder.TotalPackageThermolabile != request.TotalPackageThermolabile);
            consolidationOrder.ConsolidatedByName = request.ConsolidatedByName;
            consolidationOrder.ConsolidatedById = request.ConsolidatedById;
            consolidationOrder.ReceivedInShippingBy = request.ReceivedInShippingByName;
            consolidationOrder.ReceivedInShippingId = request.ReceivedInShippingById;
            consolidationOrder.TotalPackage = request.TotalPackage;
            consolidationOrder.TotalPackageThermolabile = request.TotalPackageThermolabile;
            consolidationOrder.Consolidated = request.Consolidated;
            consolidationOrder.ReceptionExpeditionTime = DateTime.Now;
            _repository.Update(consolidationOrder);
            try
            {
                await _repository.UnitOfWork.SaveChangesAsync();
                if (!request.Consolidated || request.ReceivedInShippingById.IsNullOrEmpty() ||
                    totalPackageChanged)
                {
                    await _commandBus.SendAsync(new PrintConsolidationOrderLabelCommand { Id = consolidationOrder.Id }, cancellationToken);
                }
                if (!_model.AXInterfacing)
                {
                    var deliveryOrder = await _deliveryOrderRepository.Table.FirstOrDefaultAsync(c => c.OrderId == consolidationOrder.OrderId, cancellationToken); 
                    if(deliveryOrder == null) 
                        await _commandBus.SendAsync(new CreateDeleiveryOrderCommand { OrderId = consolidationOrder.OrderId}, cancellationToken); 

                } 
                await _commandBus.SendAsync(new UpdateOrderStatusCommand { Id = consolidationOrder.OrderId, OrderStatus = 110 }, cancellationToken);
                if(request.Consolidated && !request.ReceivedInShippingById.IsNullOrEmpty())
                    await _commandBus.SendAsync(new MakeOrderAsToBeShippedCommand { OrderId = consolidationOrder.OrderId }, cancellationToken);
                #region Change Colisage sur AX
                if (totalPackageChanged)
                {
                    var order = await _commandBus.SendAsync(new GetSharedOrderById { OrderId = consolidationOrder.OrderId }, cancellationToken);
                    if(!await UpdateTotalPackagesAX(order.CodeAx, consolidationOrder.TotalPackage
                        , consolidationOrder.TotalPackageThermolabile, cancellationToken))
                    {
                        throw new Exception("Erreur survenue lors de la modification du nombre de colis sur AX");
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Bl introuvable", e.Message)
                    }
                };
            }
            return default;
        }

        private async Task<bool> UpdateTotalPackagesAX(string codeAx,int totalPackages,int totalPackageThermolabile, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(codeAx) && totalPackages+totalPackageThermolabile>0)
            {
                DOSI_SalesOrderServiceClient client = new DOSI_SalesOrderServiceClient();
                CallContext callContext = new CallContext();
                callContext.Company = "HP";
                client.ClientCredentials.Windows.ClientCredential.UserName = _model.UserAx;
                client.ClientCredentials.Windows.ClientCredential.Password = _model.PasswordAx; // Code société dans AX
                var msg = await client.updatePackageAsync(callContext, codeAx,totalPackages,totalPackageThermolabile);
                // Voir les messages d'erreur
                return msg.response;
            }

            return false;
        }

        private static void AddCellToBody(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.NORMAL, 8, 1, BaseColor.BLACK)))
            {

                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 2,
                BackgroundColor = BaseColor.WHITE,
            });
        }
     
        private bool PrintLabel(LabelConsolidationModel labelConsolidationModel)
        {
#if DEBUG
            return true;
#endif
            try
            {
                var template = LabelZplHelper.GetZplLabelTemplate(labelConsolidationModel);
                if (_printerOptions.RawPrinters != null)
                {
                    var printerName = _printerOptions.RawPrinters[0];

                    return RawPrinterHelper.SendStringToPrinter(printerName, template); 
               }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }           
        }

        public async Task<ValidationResult> Handle(PrintConsolidationOrderLabelCommand request, CancellationToken cancellationToken)
        {
            var consolidationOrder = await _repository.Table.Where(c => request.Id == c.Id).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            if (consolidationOrder.TotalPackage + consolidationOrder.TotalPackageThermolabile == 0)
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("NB Colis",
                         @$"toutes les lignes de la commande numéro {consolidationOrder.OrderIdentifier} ont été barrées")
                    }
                };
            var order = await _commandBus.SendAsync(new GetSharedOrderById { OrderId = consolidationOrder.OrderId }, cancellationToken);
            var customer = await _commandBus.SendAsync(new GetCustomerByOrganizationIdQuery { OrganizationId = order.CustomerId.Value }, cancellationToken);
            var blsOrder = await _preparationOrderRepository.Table
                .Where(c => c.OrderId == order.Id)
                .Include(c => c.PreparationOrderItems)
                .ToListAsync(cancellationToken: cancellationToken);
            string zones = default ;
            var index = 1;

            foreach (var op in blsOrder)
            {
                
                zones += string.Join(",", op.PreparationOrderItems.SelectMany(c => c.PickingZoneName).Distinct()) ;
                if (index < blsOrder.Count)
                    zones += ", ";
                index++;
            }
            LabelConsolidationModel label = new LabelConsolidationModel
            {
                Customername = consolidationOrder.CustomerName,
                Orderidentifier = consolidationOrder.OrderIdentifier,
                Totalpackage = consolidationOrder.TotalPackage,
                Totalpackagethermolabile = consolidationOrder.TotalPackageThermolabile,
                sector = customer.Sector + ' ' + customer.SectorCode,
                Zones = zones,
                Barcode = "0000000000".Substring(0, 10 - order.OrderNumberSequence.ToString().Length) + order.OrderNumberSequence + order.OrderNumberSequence
            };
            var response=PrintLabel(label);
            if (response)
                return default;
            else
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Erreur d'impression de l'étiquette",
                         @$"Erreur survenue lors de l'impression de l'étiquette de consolidation
pour la commande numéro {order.OrderNumberSequence.ToString()}")
                    }
                };
                
        }
    }
}
