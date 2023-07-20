using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Common;
using GHPCommerce.Core.Shared.Contracts.Indentity.Queries;
using GHPCommerce.Core.Shared.Contracts.Orders.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS.Print;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Modules.PreparationOrder.Repositories;
using Microsoft.EntityFrameworkCore;
using GHPCommerce.Core.Shared.Contracts.ZoneGroup.Queries;
using GHPCommerce.Core.Shared.PreparationOrder.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Infra.OS;
using GHPCommerce.Modules.PreparationOrder.Commands.Consolidation;
using GHPCommerce.Modules.PreparationOrder.Helpers;
using GHPCommerce.Application.Catalog.Products.Queries; 
using System.Text.Json.Serialization;
using GHPCommerce.Core.Shared.Contracts.PickingZone.Queries;
using GHPCommerce.Domain.Domain.Catalog;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace GHPCommerce.Modules.PreparationOrder.Commands
{
    public class PreparationOrderCommandHandler :
        ICommandHandler<CreatePreparationOrderCommand, ValidationResult>,
        ICommandHandler<PrintBlCommand, ValidationResult>,
        ICommandHandler<PrintBulkBlCommand, ValidationResult>,
        ICommandHandler<PrintBulkPendingCommand, ValidationResult>,
        ICommandHandler<PrintPreparationOrderCommand, ValidationResult>,
     
        ICommandHandler<AddAgentsCommand, ValidationResult>,
        ICommandHandler<ZoneConsolidationCommand, ValidationResult>,
        ICommandHandler<UpdatePreparationOrderItem, ValidationResult>,
        ICommandHandler<CancelPreparationsForOrderCommand, ValidationResult>


    {
        private readonly IMapper _mapper;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly IPreparationOrderRepository _preparationOrderRepository;
        private readonly IRepository<ConsolidationOrder, Guid> _consolidationRepository;
        private readonly ICurrentUser _currentUser;
        private readonly OpSettings _opSettings;
        private readonly Logger _logger;

        private readonly ISequenceNumberService<Entities.PreparationOrder, Guid> _sequenceNumberService;
        
        public PreparationOrderCommandHandler(
           IMapper mapper,
           ICommandBus commandBus,
           ICurrentOrganization currentOrganization,
           IPreparationOrderRepository preparationOrderRepository,
           IRepository<ConsolidationOrder, Guid> consolidationRepository,
           ICurrentUser currentUser,
           ISequenceNumberService<Entities.PreparationOrder, Guid> sequenceNumberService,
           OpSettings model
, Logger logger)

        {
            _mapper = mapper;
            _commandBus = commandBus;
            _currentOrganization = currentOrganization;
            _currentUser = currentUser;
            _preparationOrderRepository = preparationOrderRepository;
            _consolidationRepository = consolidationRepository;
            _sequenceNumberService = sequenceNumberService;
            _opSettings = model;
            _logger = logger;
        }

        public async Task<ValidationResult> Handle(CreatePreparationOrderCommand request, CancellationToken cancellationToken)
        {
            var currentOrganizationId = await _currentOrganization.GetCurrentOrganizationIdAsync();
            if (currentOrganizationId == null)
                throw new InvalidOperationException();
            var keysq = nameof(Entities.PreparationOrder) + currentOrganizationId;
            try
            {
                var preparationOrder = _mapper.Map<Entities.PreparationOrder>(request);
                preparationOrder.OrganizationId = currentOrganizationId.Value;
                preparationOrder.OrganizationName = await _currentOrganization.GetCurrentOrganizationNameAsync();

                await LockProvider<string>.ProvideLockObject(keysq).WaitAsync( cancellationToken);
                var sq = await _sequenceNumberService.GenerateSequenceNumberAsync(DateTime.Now,
                    currentOrganizationId.Value);
                preparationOrder.SequenceNumber = sq;

                preparationOrder.IdentifierNumber = "BL-" + DateTime.Now.Year.ToString().Substring(0, 2) +
                                                    "0000000000".Substring(0,
                                                        9 - preparationOrder.OrderIdentifier.Length) +
                                                    preparationOrder.OrderIdentifier +
                                                    "00".Substring(0, 1 - request.zoneGroupOrder.ToString().Length) +
                                                    request.zoneGroupOrder;
                preparationOrder.BarCode = DateTime.Now.Year.ToString().Substring(2, 2) +
                                           "0000000000".Substring(0,
                                               9 - preparationOrder.OrderIdentifier.Length) +
                                           preparationOrder.OrderIdentifier +
                                           "00".Substring(0, 2 - request.zoneGroupOrder.ToString().Length) +
                                           request.zoneGroupOrder;
                var XZone = await _commandBus.SendAsync(
    new GetPickingZoneByNameQuery { ZoneName = "X" }, cancellationToken);


                foreach (var item in preparationOrder.PreparationOrderItems)
                {
                    //Some lines have ZoneGroupId null in production, from orderitem(order number 4509)


        if (string.IsNullOrEmpty(item.DefaultLocation) ||
        string.IsNullOrEmpty(item.PickingZoneName)
        ||
        item.ZoneGroupId == null || item.ZoneGroupId == Guid.Empty)
        {
            var product = await _commandBus.SendAsync(new GetProductByIdQuery
            {
                Id = item.ProductId
            });
            if (product != null)
            {
                if (product.PickingZone == null)
                {
                    product.PickingZone = new PickingZone
                    {
                        Name = XZone.Name,
                        Id = XZone.Id,
                        ZoneGroup =
                        new ZoneGroup(XZone.ZoneGroup.Id,
                        XZone.ZoneGroup.Name, null, XZone.ZoneGroup.Order, XZone.ZoneGroup.Printer
                        ),
                        ZoneGroupId = XZone.ZoneGroupId,
                        Order = XZone.Order
                    };
                }
                item.ZoneGroupId = product.PickingZone.ZoneGroupId;
                item.ZoneGroupName = product.PickingZone.ZoneGroup.Name;
                item.PickingZoneId = product.PickingZone.Id;
                item.PickingZoneName = product.PickingZone.Name;
                item.PickingZoneOrder = product.PickingZone.Order;
                item.DefaultLocation = product.DefaultLocation??"X";

            }

        }
  


                    item.Status = BlStatus.Valid;
                    item.OldQuantity = item.Quantity;
                    item.PackingQuantity = (item.Packing != 0) ? item.Quantity / item.Packing : 0;
                    item.PreviousInternalBatchNumber = item.InternalBatchNumber;
                }

                preparationOrder.TotalPackage =
                    preparationOrder.PreparationOrderItems.Sum(c => c.PackingQuantity.Value);
                _preparationOrderRepository.Add(preparationOrder);

                await _preparationOrderRepository.UnitOfWork.SaveChangesAsync(); 
                LockProvider<string>.ProvideLockObject(keysq).Release();
            }
            catch (Exception e)
            {
                LockProvider<string>.ProvideLockObject(keysq).Release();
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Erreur survenue lors de la création des OPS",e.Message )
                    }
                };
            }

            return default;
        }

        public async Task<ValidationResult> Handle(PrintPreparationOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await LockProvider<string>.ProvideLockObject("print").WaitAsync(cancellationToken);
            var preparationOrders = await _preparationOrderRepository.Table
                //.AsNoTracking()
                .Where(c => request.OrdersIds.Any(o => o == c.OrderId))
                .Include(c => c.PreparationOrderItems)
                .OrderBy(c => c.OrderIdentifier)
                .ThenBy(c => c.CustomerName)
                .ThenBy(c => c.ZoneGroupName)
                .ThenBy(c => c.ZoneGroupOrder)
                .ToListAsync(cancellationToken: cancellationToken);

            var oldOrderId = Guid.Empty;
            var results = new ValidationResult();
                //validationErrors.Errors.Add(new ValidationFailure("Impression échouée", "Cette commande a été  annulée par le service commercial."));
            for(int i=0;i<preparationOrders.Count;i++)
            {
                var item = preparationOrders[i];
            
                oldOrderId = item.OrderId;
                var result = await _commandBus.SendAsync(new PrintBlCommand { Id = item.Id ,Bulk=true }, cancellationToken);
                if (result != default)
                    result.Errors.ToList().ForEach(er => {
                        if (!results.Errors.Any(r => r.ErrorMessage == er.ErrorMessage))
                            results.Errors.Add(er);
                            });

                    if (result == default)
                    {
                        var id = preparationOrders[i].Id;
                        var preparationOrder = await _preparationOrderRepository.Table
                            
                        .FirstOrDefaultAsync(c => id == c.Id);
                        if (preparationOrder == null) continue;
                        var currentUser =
                            await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                            cancellationToken);
                        preparationOrder.PrintedTime = DateTime.Now;
                        preparationOrder.PrintedBy = _currentUser.UserId;
                        preparationOrder.PrintedByName = currentUser.NormalizedUserName;
                        preparationOrder.Printed = true;
                        preparationOrder.PrintCount += 1;
                        _preparationOrderRepository.Update(preparationOrder);
                    }
                }
                try
                {
                    
                    await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
                    LockProvider<string>.ProvideLockObject("print").Release();
                }
                catch (Exception ex)
                {
                    LockProvider<string>.ProvideLockObject("print").Release();
                    return new ValidationResult
                    {
                        Errors =
                    {
                        new ValidationFailure("Erreur survenue ",ex.Message )
                    }
                    };
                }
                if (results.Errors != null && results.Errors.Count > 0)
                {
                    
                    return results;
                }
            }
            catch (Exception ex)
            {
                if (LockProvider<string>.ProvideLockObject("print").CurrentCount>0)
                LockProvider<string>.ProvideLockObject("print").Release();
                _logger.Error("Erreur survenue ", ex.Message);
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Erreur survenue ",ex.Message )
                    }
                };
            } 
            return default;
        }

      

        public async Task<ValidationResult> Handle(PrintBlCommand request, CancellationToken cancellationToken)
        {
            var currentUser =
                await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                    cancellationToken);
            var preparationOrderQuery =  _preparationOrderRepository.Table;
            if (request.Bulk)
                preparationOrderQuery = preparationOrderQuery.AsNoTracking();
                var preparationOrder=await (preparationOrderQuery
                .Where(c => request.Id == c.Id)
                .Include(c => c.PreparationOrderItems)
                .Include(c=>c.PreparationOrderVerifiers)
                .Include(c => c.PreparationOrderExecuters)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken));
            #region calculate page number in PO & first page number in delivery order
            if (string.IsNullOrEmpty(request.ZonesStringByBL))
            {
                int pageCountPerOrder = 0;

                var pagePerZoneGroup = new Dictionary<Guid, int>();
                var relatedPreparationOrders = await _preparationOrderRepository.Table
                    .AsNoTracking()
        .Where(c => preparationOrder.OrderId == c.OrderId)
        .OrderBy(c => c.ZoneGroupOrder)
        .ThenBy(c => c.ZoneGroupName)
        .Include(c => c.PreparationOrderItems)
        .ToListAsync(cancellationToken: cancellationToken);
   
                request.ZonesStringByBL =
                String.Join(" - ",
                relatedPreparationOrders.Select(r =>
                  String.Join("/",
                r.PreparationOrderItems.Where(r=>!string.IsNullOrEmpty(r.PickingZoneName)).OrderBy(p => p.PickingZoneOrder).ThenBy(p=>p.PickingZoneName)
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
                        if (item2[0].PickingZoneName.ToUpper() == "A")
                            ;
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


                        foreach (var zoneitem in item2)
                        {
                           
                            if (i> lineCountPerPage - 1)
                            {
                                i = 0;
                                height = 50+120 + 20;
                                pageCount++;
                                lineCountPerPage= initialLineCountPerPage + 160/40;
                            }
                            if (string.IsNullOrEmpty(zoneitem.PickingZoneName)) continue;
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
                    request.FirstPageNumber+=pagePerZoneGroup[item.ZoneGroupId] ;
                } 
            }
 

            #endregion
            preparationOrder.PreparationOrderItems= preparationOrder.PreparationOrderItems
                .Where(i=>!string.IsNullOrEmpty( i.PickingZoneName))
                .OrderBy(c => string.IsNullOrEmpty(c.DefaultLocation)?"ZZZ": c.DefaultLocation).ThenBy(c=>c.ProductName).ToList();
        var order = await _commandBus.SendAsync(new GetOrderByIdQueryV2 { Id = preparationOrder.OrderId },
                cancellationToken);
            if (order.OrderStatus == 70)
            {
                if (request.Bulk)
                {
                    var validationErrors = new ValidationResult();
                    validationErrors.Errors.Add(new ValidationFailure("Impression échouée", "Cette commande a été  annulée par le service commercial."));
                    return validationErrors;
                }
                throw new InvalidOperationException("Cette commande a été  annulée par le service commercial.");
            }


            if (_opSettings.ByPassControlStep)
            {
                preparationOrder.PreparationOrderStatus = PreparationOrderStatus.Controlled;
            }

            try
            {
                var zone = await _commandBus.SendAsync(new GetGroupZoneByIdQuery { Id = preparationOrder.ZoneGroupId },
                    cancellationToken);
                var pdfHelper = new PreparationOrderToPdfHelper(preparationOrder, _commandBus, request);
                preparationOrder.PrintedTime = DateTime.Now;
                preparationOrder.PrintedBy = _currentUser.UserId;
                preparationOrder.PrintedByName = currentUser?.NormalizedUserName;
                preparationOrder.Printed = true;
                preparationOrder.PrintCount += 1;

                var bytes = await pdfHelper.GeneratePreparationOrderToPdf();
#if DEBUG
                zone.Printer = "172.18.1.214";
#endif
                if (await print(bytes, zone.Printer))
                {
                //    ;
                //}
                //if (await print(bytes, zone.Printer,pageCount=pageCount))
                //{
                    if (!request.Bulk)
                    {
                        _preparationOrderRepository.Update(preparationOrder);
                        await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
                    }
                }
                else
                {
                    var validationErrors = new ValidationResult();
                    validationErrors.Errors.Add(new ValidationFailure("Impression échouée", "Problème survenu lors de l'impression"));
                    return validationErrors;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                _logger.Error(e.InnerException?.Message);
                var validationErrors = new ValidationResult();
                
                validationErrors.Errors.Add(new ValidationFailure("Customer not found ", e.Message));
                return validationErrors;
            }

            return default;
        }

        public async Task<ValidationResult> Handle(PrintBulkBlCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await LockProvider<string>.ProvideLockObject("print").WaitAsync(cancellationToken);
                for (int i = 0; i < request.Ids.Count; i++)
                {
                    var id = request.Ids[i];
                    var result = await _commandBus.SendAsync(new PrintBlCommand { Id = id, Bulk = true }, cancellationToken);
                    if (result == default)
                    {
                        var preparationOrder = await _preparationOrderRepository.Table
                            //.AsNoTracking()
                        .FirstOrDefaultAsync(c => id == c.Id);
                        if (preparationOrder == null) continue;
                        var currentUser =
                            await _commandBus.SendAsync(new GetUserQuery { Id = _currentUser.UserId, IncludeRoles = true },
                            cancellationToken);
                        preparationOrder.PrintedTime = DateTime.Now;
                        preparationOrder.PrintedBy = _currentUser.UserId;
                        preparationOrder.PrintedByName = currentUser.NormalizedUserName;
                        preparationOrder.Printed = true;
                        preparationOrder.PrintCount += 1;
                        _preparationOrderRepository.Update(preparationOrder);
                    }
                }
                try
                {
                    await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
                    LockProvider<string>.ProvideLockObject("print").Release();
                }
                catch (Exception ex)
                {
                    LockProvider<string>.ProvideLockObject("print").Release();
                    return new ValidationResult
                    {
                        Errors =
                    {
                        new ValidationFailure("Erreur survenue ",ex.Message )
                    }
                    };
                }
            }
            catch (Exception ex)
            {
                LockProvider<string>.ProvideLockObject("print").Release();
                _logger.Error("Erreur survenue ", ex.Message);
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Erreur survenue ",ex.Message )
                    }
                };
            } 
            return default;
        }

        public async Task<ValidationResult> Handle(PrintBulkPendingCommand request, CancellationToken cancellationToken)
        {
            await LockProvider<string>.ProvideLockObject("pending").WaitAsync(cancellationToken);

            var query = _preparationOrderRepository.Table.AsNoTracking()
                .Where(c => !c.Printed && c.PreparationOrderStatus != PreparationOrderStatus.CancelledOrder);
                 

            if (request.ZoneGroupName != null) query = query.Where(c => c.ZoneGroupName.Contains(request.ZoneGroupName));
            if (request.CustomerName != null) query = query.Where(c => c.CustomerName.Contains(request.CustomerName));
            if (request.OrganizationName != null) query = query.Where(c => c.OrganizationName.Contains(request.OrganizationName));
            if (request.SectorName != null) query = query.Where(c => c.SectorName.Contains(request.SectorName));
            var preparationOrdersIds = await query
                .OrderBy(c=>c.OrderIdentifier)
                .ThenBy(c => c.ZoneGroupName)
                .ThenBy(c => c.ZoneGroupOrder)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken: cancellationToken);
            await _commandBus.SendAsync(new PrintBulkBlCommand { Ids = preparationOrdersIds }, cancellationToken);
            LockProvider<string>.ProvideLockObject("pending").Release();
            return default;

        }

         public async Task<ValidationResult> Handle(AddAgentsCommand request, CancellationToken cancellationToken)
        {
            // var blItems = await _preparationOrderRepository.Table.Where(c => c.Id == request.PreparationOrderId)
            //                .Include(c => c.PreparationOrderItems)
            //                .SelectMany(c => c.PreparationOrderItems).ToListAsync();

            var bl = await _preparationOrderRepository.Table
                        .AsTracking()
                        .Include(c => c.PreparationOrderItems)
                        .Include(c => c.PreparationOrderExecuters)
                        .Include(c => c.PreparationOrderVerifiers)
                        .FirstOrDefaultAsync(c => c.Id == request.PreparationOrderId, cancellationToken);
            if (bl == null)
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Bl Not Found", "Id Bl N'existe pas !")
                    }
                };
            // foreach (var item in bl.PreparationOrderItems)
            // {
            //     if (item.PickingZoneId == request.PickingZoneId) item.IsControlled = true;
            // }
            if (bl.PreparationOrderExecuters.Any() && bl.PreparationOrderExecuters.Any(c => c.PickingZoneId == request.PickingZoneId))
                foreach (var item in bl.PreparationOrderExecuters)
                {
                    if (item.PickingZoneId == request.PickingZoneId)
                    {
                        item.ExecutedById = request.ExecutedById;
                        item.ExecutedByName = request.ExecutedByName;
                        item.ExecutedTime = DateTime.Now;

                    }
                } 
            else {
                var preparationOrderExecutor = new PreparationOrderExecuter
                {
                    PreparationOrderId = request.PreparationOrderId,
                    ExecutedById = request.ExecutedById,
                    ExecutedByName = request.ExecutedByName,
                    ExecutedTime = DateTime.Now,
                    PickingZoneId = request.PickingZoneId,
                    PickingZoneName = request.PickingZoneName

                };
                bl.PreparationOrderExecuters.Add(preparationOrderExecutor);
            }
            if (bl.PreparationOrderVerifiers.Any() && bl.PreparationOrderVerifiers.Any(c => c.PickingZoneId == request.PickingZoneId)) { 
                foreach (var item in bl.PreparationOrderVerifiers)
                {
                    if (item.PickingZoneId == request.PickingZoneId)
                    {
                        item.VerifiedById = request.VerifiedById;
                        item.VerifiedByName = request.VerifiedByName;
                        item.VerifiedTime = DateTime.Now;

                    }
                } 
            } else {
                  var preparationOrderVerifier = new PreparationOrderVerifier
                  {
                      PreparationOrderId = request.PreparationOrderId,
                      VerifiedById = request.VerifiedById,
                      VerifiedByName = request.VerifiedByName,
                      VerifiedTime = DateTime.Now,
                      PickingZoneId = request.PickingZoneId,
                      PickingZoneName = request.PickingZoneName
                  };
                bl.PreparationOrderVerifiers.Add(preparationOrderVerifier);
            }
            _preparationOrderRepository.Update(bl);
            try
            {
                await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                _logger.Error("Erreur lors de l'affectaion des agents : " + e.Message);
                throw e;
            }
            return default;
        }
        //public async Task<ValidationResult> Handle(ZoneConsolidationCommand request, CancellationToken cancellationToken)
        //{

        //    var blItems = await _preparationOrderRepository.Table
        //        .Where(c => c.Id == request.PreparationOrderId)
        //        .Include(c => c.PreparationOrderItems)
        //        .SelectMany(c => c.PreparationOrderItems).
        //        ToListAsync(cancellationToken: cancellationToken);


        //    var bl = await _preparationOrderRepository.Table.FirstOrDefaultAsync(c => c.Id == request.PreparationOrderId, cancellationToken);
        //    if (bl == null)
        //        return new ValidationResult
        //        {
        //            Errors =
        //            {
        //                new ValidationFailure("Bl Not Found", "Id Bl N'existe pas !")
        //            }
        //        };
        //    bl.TotalPackage = request.TotalPackage;

        //    bl.TotalPackageThermolabile = request.TotalPackageThermolabile;
        //    bl.EmployeeCode = request.EmployeeCode;
        //    bl.ConsolidatedById = request.ConsolidatedById;
        //    bl.ConsolidatedByName = request.ConsolidatedByName;
        //    bl.ConsolidatedTime = DateTime.Now;
        //    _preparationOrderRepository.Update(bl);
        //    try
        //    {
        //        await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
                
        //        List<Guid> zoneConsolidated;
          
        //        if (_opSettings.ByPassControlStep)
        //        {
        //          var query =    await _preparationOrderRepository.Table
        //                .AsNoTracking()
        //              .Where(c => c.Id == request.PreparationOrderId)
        //              .Include(c => c.PreparationOrderItems)
        //              .SelectMany(c => c.PreparationOrderItems)
        //              .ToListAsync(cancellationToken: cancellationToken);
        //           zoneConsolidated = query.Select(c => c.PickingZoneId.Value).ToList();
        //        }
        //        else
        //        {
        //            var itemsControlled = await _preparationOrderRepository.Table
        //                .AsNoTracking()
        //                .Where(c => c.Id == request.PreparationOrderId)
        //                .Include(c => c.PreparationOrderItems)
        //                .Where(c=>c.PreparationOrderStatus==PreparationOrderStatus.Consolidated)
        //                .SelectMany(c => c.PreparationOrderItems).ToListAsync();
        //            zoneConsolidated = itemsControlled.Select(c => c.PickingZoneId??Guid.Empty).Distinct().ToList();

        //            var itemsControlled2 = await _preparationOrderRepository.Table
        //                .Where(c => c.Id == request.PreparationOrderId)
        //                .Include(c => c.PreparationOrderVerifiers).SelectMany(c => c.PreparationOrderVerifiers).ToListAsync();

        //            var zoneConsolidated2 = itemsControlled.Select(c => c.PickingZoneId).Distinct().ToList();
        //        }

        //        var zonesToConsolidate = blItems.Select(c => c.PickingZoneId).Distinct().ToList();
        //        if (zoneConsolidated.Count() == zonesToConsolidate.Count)
        //        {
        //            var consolidationOrder = await _consolidationRepository.Table.AsNoTracking().Where(c => c.OrderId == bl.OrderId)
        //                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        //            if (consolidationOrder == null) {
        //                bl.PreparationOrderStatus = PreparationOrderStatus.Consolidated;
        //                bl.ConsolidatedTime = DateTime.Now;
        //                _preparationOrderRepository.Update(bl);
        //                await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
                  
        //                var blsOrder = await _preparationOrderRepository.Table
        //                    .AsNoTracking()
        //                    .Where(c => c.OrderId == bl.OrderId)
        //                    .ToListAsync(cancellationToken: cancellationToken);
        //                var countBlConsolidated = blsOrder.Count(c => c.PreparationOrderStatus == PreparationOrderStatus.Consolidated);
        //                if (blsOrder.Count == countBlConsolidated)
        //                {
        //                    var consolidationCommand = new ConsolidationCommand
        //                    {
        //                        CustomerName = bl.CustomerName,
        //                        CustomerId = bl.CustomerId,
        //                        OrganizationName = bl.OrganizationName,
        //                        OrganizationId = bl.OrganizationId,
        //                        OrderId = bl.OrderId,
        //                        OrderDate = bl.OrderDate,
        //                        OrderIdentifier = bl.OrderIdentifier,
        //                        TotalPackage = blsOrder.Sum(c => c.TotalPackage),
        //                        TotalPackageThermolabile = blsOrder.Sum(c => c.TotalPackageThermolabile)
        //                    };
        //                    var res =await _commandBus.SendAsync(consolidationCommand, cancellationToken);
        //                    if (res != null && !res.IsValid)
        //                        return res;

        //                }
        //            } else
        //            {
        //                var order = await _commandBus.SendAsync(new GetSharedOrderById {OrderId = bl.OrderId}, cancellationToken);
        //                if (order.OrderStatus == 100 || order.OrderStatus == 110 || order.OrderStatus == 140)
        //                {
        //                    var blsOrder = await _preparationOrderRepository.Table
        //                        .AsNoTracking()
        //                        .Where(c => c.OrderId == bl.OrderId)
        //                        .ToListAsync(cancellationToken: cancellationToken);
        //                    consolidationOrder.TotalPackage = blsOrder.Sum(c => c.TotalPackage);
        //                    consolidationOrder.TotalPackageThermolabile = blsOrder.Sum(c => c.TotalPackageThermolabile);
        //                    await _commandBus.SendAsync(_mapper.Map<ConsolidationUpdateCommand>(consolidationOrder), cancellationToken);

        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        var validationErrors = new ValidationResult();
        //        validationErrors.Errors.Add(new ValidationFailure("Erreur lors de la consolidation/facturation",
        //            e.Message));
        //        return validationErrors;
                
        //    }
        //    return default;

        //}

        public async Task<ValidationResult> Handle(ZoneConsolidationCommand request, CancellationToken cancellationToken)
        {

            var blItems = await _preparationOrderRepository.Table.Where(c => c.Id == request.PreparationOrderId)
                .Include(c => c.PreparationOrderItems)
                .SelectMany(c => c.PreparationOrderItems).ToListAsync(cancellationToken: cancellationToken);


            var bl = await _preparationOrderRepository.Table.AsTracking().FirstOrDefaultAsync(c => c.Id == request.PreparationOrderId, cancellationToken);
            if (bl == null)
                return new ValidationResult
                {
                    Errors =
                    {
                        new ValidationFailure("Bl Not Found", "Id Bl N'existe pas !")
                    }
                };
            bl.TotalPackage = request.TotalPackage;

            bl.TotalPackageThermolabile = request.TotalPackageThermolabile;
            bl.EmployeeCode = request.EmployeeCode;
            bl.ConsolidatedById = request.ConsolidatedById;
            bl.ConsolidatedByName = request.ConsolidatedByName;
            bl.ConsolidatedTime = DateTime.Now;
            _preparationOrderRepository.Update(bl);
            try
            {
                await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
                List<Guid> zoneConsolidated;

                if (_opSettings.ByPassControlStep)
                {
                    var query = await _preparationOrderRepository.Table
                        .Where(c => c.Id == request.PreparationOrderId)
                        .Include(c => c.PreparationOrderItems)
                        .SelectMany(c => c.PreparationOrderItems)
                        .ToListAsync();
                    zoneConsolidated = query.Select(c => c.PickingZoneId.Value).ToList();
                }
                else
                {
                    var itemsControlled = await _preparationOrderRepository.Table
                        .Where(c => c.Id == request.PreparationOrderId)
                        .Include(c => c.PreparationOrderVerifiers).SelectMany(c => c.PreparationOrderVerifiers).ToListAsync();

                    zoneConsolidated = itemsControlled.Select(c => c.PickingZoneId).Distinct().ToList();

                }

                var zonesToConsolidate = blItems.Select(c => c.PickingZoneId).Distinct().ToList();
                if (zoneConsolidated.Count() == zonesToConsolidate.Count)
                {
                    var consolidationOrder = await _consolidationRepository.Table.Where(c => c.OrderId == bl.OrderId)
                        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                    if (consolidationOrder == null)
                    {
                        bl.PreparationOrderStatus = PreparationOrderStatus.Consolidated;
                        bl.ConsolidatedTime = DateTime.Now;
                        _preparationOrderRepository.Update(bl);
                        await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();

                        var blsOrder = await _preparationOrderRepository.Table
                            .Where(c => c.OrderId == bl.OrderId)
                            .ToListAsync(cancellationToken: cancellationToken);
                        var countBlConsolidated = blsOrder.Count(c => c.PreparationOrderStatus == PreparationOrderStatus.Consolidated);
                        if (blsOrder.Count == countBlConsolidated)
                        {
                            var consolidationCommand = new ConsolidationCommand
                            {
                                CustomerName = bl.CustomerName,
                                CustomerId = bl.CustomerId,
                                OrganizationName = bl.OrganizationName,
                                OrganizationId = bl.OrganizationId,
                                OrderId = bl.OrderId,
                                OrderDate = bl.OrderDate,
                                OrderIdentifier = bl.OrderIdentifier,
                                TotalPackage = blsOrder.Sum(c => c.TotalPackage),
                                TotalPackageThermolabile = blsOrder.Sum(c => c.TotalPackageThermolabile)
                            };
                            await _commandBus.SendAsync(consolidationCommand, cancellationToken);

                        }
                    }
                    else
                    {
                        var order = await _commandBus.SendAsync(new GetSharedOrderById { OrderId = bl.OrderId }, cancellationToken);
                        if (order.OrderStatus == 100 || order.OrderStatus == 110 || order.OrderStatus == 140)
                        {
                            var blsOrder = await _preparationOrderRepository.Table
                                .Where(c => c.OrderId == bl.OrderId)
                                .ToListAsync(cancellationToken: cancellationToken);
                            consolidationOrder.TotalPackage = blsOrder.Sum(c => c.TotalPackage);
                            consolidationOrder.TotalPackageThermolabile = blsOrder.Sum(c => c.TotalPackageThermolabile);
                            await _commandBus.SendAsync(_mapper.Map<ConsolidationUpdateCommand>(consolidationOrder), cancellationToken);

                        }
                    }
                }
            }
            catch (Exception e)
            {

                throw e;
            }
            return default;

        }
        public static async Task<bool> print(byte[] fileBytes, string printerIP,int pageCount=0 )
        {
            try
            {
                
                PrintHelper printHelper = new PrintHelper(fileBytes, printerIP);
                return await printHelper.PrintData();
              
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.Message, "Error");
            }
            return false;

        }

        public async Task<ValidationResult> Handle(UpdatePreparationOrderItem request, CancellationToken cancellationToken)
        {
            var op = await _preparationOrderRepository.Table
                .AsNoTracking()
                .Include(c => c.PreparationOrderItems)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            foreach (var item in op.PreparationOrderItems)
            {
                item.OldQuantity = item.Quantity;
                item.Quantity = request.Quantity;
                
            }
            _preparationOrderRepository.Update(op);
            try
            {
                await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                
                throw e;
            }
            return default;

        }

        public async Task<ValidationResult> Handle(CancelPreparationsForOrderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var preparationOrders = await _preparationOrderRepository.Table
                    .AsNoTracking()
                    .Where(c => c.OrderId == request.OrderId).ToListAsync(cancellationToken);
                if (preparationOrders == null || preparationOrders.Count == 0) return default;
                preparationOrders.ForEach(p => p.PreparationOrderStatus = PreparationOrderStatus.CancelledOrder);
                await _preparationOrderRepository.UnitOfWork.SaveChangesAsync();
                return default;
            }
            catch(Exception ex)
            {
                throw new Exception("Echec d'annulation des préparations");
                
            }

        }
    }
}
