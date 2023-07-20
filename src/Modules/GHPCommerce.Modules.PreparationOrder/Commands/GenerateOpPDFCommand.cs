using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.PreparationOrder.DTOs;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Modules.PreparationOrder.Helpers;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public  interface  IPDFCommande
    {
        public Guid Id { get; set; }
        public int PageCount { get; set; }
        public int FirstPageNumber { get; set; }
        public int TotalPageCount { get;  set; }
        public string ZonesStringByBL { get; set; } 
        public bool Bulk { get; set; } 
        public List<string> ZonesOnTopPage { get; set; } 
    }
    public class GenerateOpPDFCommand :IPDFCommande, ICommand<PrintPdfDto>
    {
        public Guid Id { get; set; }
        public int PageCount { get; set; }
        public int FirstPageNumber { get; set; }
        public int TotalPageCount { get;  set; }
        public string ZonesStringByBL { get; set; } = "";
        public bool Bulk { get; set; } = false;
        public List<string> ZonesOnTopPage { get; set; } = new List<string>();
    }

    public class GenerateOpPDFCommandHandler : ICommandHandler<GenerateOpPDFCommand, PrintPdfDto>
    {
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IPreparationOrderRepository _preparationOrderRepository;
        private readonly IRepository<ConsolidationOrder, Guid> _consolidationRepository;
        private readonly ICurrentUser _currentUser;
        private readonly OpSettings _model;
        private readonly Logger _logger;

        public GenerateOpPDFCommandHandler(
            IMapper mapper,
            ICommandBus commandBus,
            ICurrentOrganization currentOrganization,
            IPreparationOrderRepository preparationOrderRepository,
            IRepository<ConsolidationOrder, Guid> consolidationRepository,
            ICurrentUser currentUser,
            OpSettings model,
            Logger logger)
        {
            _mapper = mapper;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _preparationOrderRepository = preparationOrderRepository;
            _consolidationRepository = consolidationRepository;
            _currentUser = currentUser;
            _model = model;
            _logger = logger;
        }

        public async Task<PrintPdfDto> Handle(GenerateOpPDFCommand request, CancellationToken cancellationToken)
        {
            
            var preparationOrder = await _preparationOrderRepository.Table
                .Where(c => request.Id == c.Id)
                .Include(c => c.PreparationOrderItems)
                .Include(c => c.PreparationOrderVerifiers)
                .Include(c => c.PreparationOrderExecuters)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            #region calculate page number in PO & first page number in delivery order

            if (string.IsNullOrEmpty(request.ZonesStringByBL))
            {
                int pageCountPerOrder = 0;

                var pagePerZoneGroup = new Dictionary<Guid, int>();
                var relatedPreparationOrders = await _preparationOrderRepository.Table
                    .Where(c => preparationOrder.OrderId == c.OrderId)
                    .OrderBy(c => c.ZoneGroupOrder)
                    .ThenBy(c => c.ZoneGroupName)
                    .Include(c => c.PreparationOrderItems)
                    .ToListAsync(cancellationToken: cancellationToken);

                request.ZonesStringByBL =
                    String.Join(" - ",
                        relatedPreparationOrders.Select(r =>
                            String.Join("/",
                                r.PreparationOrderItems.Where(r => !string.IsNullOrEmpty(r.PickingZoneName))
                                    .OrderBy(p => p.PickingZoneOrder).ThenBy(p => p.PickingZoneName)
                                    .Select(p => p.PickingZoneName).Distinct())));

         foreach (var item in relatedPreparationOrders.OrderBy(c=>c.PreparationOrderItems[0].PickingZoneName))
                {
                    #region calculate page number in PO & first page number in delivery order
                    var perZones = item.PreparationOrderItems.GroupBy(i => i.PickingZoneId).ToDictionary(g => g.Key, g => g.ToList());
                    int pageCount = 1;
                    int height = 50 + 160;
                    int lineCountPerPage = 10;
                    int initialLineCountPerPage = 10;
                    foreach (var item2 in perZones.Values.OrderBy(p=>p[0].PickingZoneName))
                    {
                        if (item2[0].PickingZoneName.ToUpper() == "A") {};
                        height += 120 + 20;
                        if (height > 842) { 
                            height = 50; 
                            pageCount++;  
                        }
                        if (height+ (item2.Count>lineCountPerPage? lineCountPerPage : item2.Count) 
                            * 40+20+10 > 842)
                        {
                            request.ZonesOnTopPage.Add(item2[0].PickingZoneName.ToUpper());
                            height = 50+120 + 20;
                            pageCount++;
                            lineCountPerPage = initialLineCountPerPage + 160 / 40;
                        }
                        int i = 0;
                        int k = 1;


                        foreach (var orderItem in item2)
                        {
                           
                            if (i> lineCountPerPage - 1)
                            {
                                i = 0;
                                height = 50+120 + 20;
                                pageCount++;
                                lineCountPerPage= initialLineCountPerPage + 160/40;
                            }
                            if (string.IsNullOrEmpty(orderItem.PickingZoneName)) continue;
                            height += 40;
                            if (height > 842 && i< lineCountPerPage-1) { 
                                height = 50;
                                pageCount++;
                                lineCountPerPage = initialLineCountPerPage + 160 / 40;
                            }
                            i++;
                            k++;
                        }
                        height += 20 + 10;
                        if (height > 842) { height = 50; pageCount++; }
                        

                    } 
                    pageCountPerOrder += pageCount; 
                    pagePerZoneGroup.TryAdd(item.ZoneGroupId, pageCount);
                    #endregion

                }
                request.TotalPageCount = pageCountPerOrder;
                request.PageCount = pagePerZoneGroup[preparationOrder.ZoneGroupId];
                request.FirstPageNumber = 1;
                foreach (var item in relatedPreparationOrders)
                {
                    if (item.ZoneGroupId == preparationOrder.ZoneGroupId)
                        break;
                    request.FirstPageNumber += pagePerZoneGroup[item.ZoneGroupId];
                }
            }


            #endregion

            preparationOrder.PreparationOrderItems = preparationOrder.PreparationOrderItems
                .Where(i => !string.IsNullOrEmpty(i.PickingZoneName))
                .OrderBy(c => string.IsNullOrEmpty(c.DefaultLocation) ? "ZZZ" : c.DefaultLocation)
                .ThenBy(c => c.ProductName).ToList();


            try
            {
                var order = await _commandBus.SendAsync(new GetOrderByIdQueryV2 { Id = preparationOrder.OrderId },
                    cancellationToken);
                if (order.OrderStatus == 70)
                {
                    throw new InvalidOperationException("Cette commande a été  annulée par le service commercial.");
                }

                if (_model.ByPassControlStep)
                {
                    preparationOrder.PreparationOrderStatus = PreparationOrderStatus.Controlled;
                }

                var pdfHelper = new PreparationOrderToPdfHelper(preparationOrder, _commandBus, request);
                var bytes = await pdfHelper.GeneratePreparationOrderToPdf();
                return new PrintPdfDto { Data = bytes, TotalPages = request.TotalPageCount };

            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message);
                _logger.Error(exception.InnerException?.Message);
                return new PrintPdfDto { Data = null, TotalPages = 0, ErrorMessage = exception.Message };
            }
            
        }
    }
}