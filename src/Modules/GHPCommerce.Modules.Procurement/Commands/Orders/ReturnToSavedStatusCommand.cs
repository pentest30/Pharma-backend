using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Cache;
using GHPCommerce.CrossCuttingConcerns.Caching;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Modules.Procurement.Entities;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace GHPCommerce.Modules.Procurement.Commands.Orders
{
    public class ReturnToSavedStatusCommand : SaveSupplierOrderCommand
    {
        
    }
     public  class ReturnToSaveStatusCommandHandler : ICommandHandler<ReturnToSavedStatusCommand, ValidationResult>
     {
         private readonly IRepository<SupplierOrder, Guid> _supplierOrderRepository;
         private readonly IMapper _mapper;
         private readonly ICurrentOrganization _currentOrganization;
         private readonly ICurrentUser _currentUser;
         private readonly ICache _redisCache;
         private readonly Logger _logger;
         private const string CACHE_KEY = "_supplier-order";

         public ReturnToSaveStatusCommandHandler(IRepository<SupplierOrder, Guid> supplierOrderRepository,
             IMapper mapper, 
             ICurrentOrganization currentOrganization, 
             ICurrentUser currentUser, 
             ICache redisCache, 
             Logger logger)
         {
             _supplierOrderRepository = supplierOrderRepository;
             _mapper = mapper;
             _currentOrganization = currentOrganization;
             _currentUser = currentUser;
             _redisCache = redisCache;
             _logger = logger;
         }
         public  async Task<ValidationResult> Handle(ReturnToSavedStatusCommand request, CancellationToken cancellationToken)
         {
             try
             {
                 var orgId = _currentOrganization.GetCurrentOrganizationIdAsync();
                 if (orgId == default) throw new InvalidOperationException("");
               
                 var order = await _supplierOrderRepository.Table
                     .Include(x => x.OrderItems)
                     .Include(x=>x.Invoices)
                     .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken: cancellationToken);
                 if (order == null || order.Invoices.Any())
                     throw new InvalidOperationException("Commande non trouvée ou liée à une ou plusieures factures");
                // back to saved status
                 order.OrderStatus = ProcurmentOrderStatus.Saved;
                 _supplierOrderRepository.Update(order);
                 await _supplierOrderRepository.UnitOfWork.SaveChangesAsync();
                 // creates a new draft order in order to enable editing 
                 var lookupKey = await GetLookupKey();
                 string key = await GetOrderKey(request.OrderId);
                 var lookUp = _redisCache.Get<List<Guid>>(lookupKey);

                 if (lookUp == null)
                     await _redisCache.AddOrUpdateAsync<List<Guid>>(lookupKey, new List<Guid> {request.OrderId}, cancellationToken);
                 else if (lookUp.All(x => x != request.OrderId))
                 {
                     lookUp.Add(request.OrderId);
                     await _redisCache.AddOrUpdateAsync<List<Guid>>(lookupKey, lookUp, cancellationToken);
                 }

                 var draftOrder = _mapper.Map<CachedOrder>(order);
                 await _redisCache.AddOrUpdateAsync<CachedOrder>(key, draftOrder, cancellationToken);
             }
             catch (Exception ex)
             {
                 var validations = new ValidationResult
                     {Errors = {new ValidationFailure("Transaction rolled back", ex.Message)}};
                 return validations;
             }

             return default;
         }
         private async Task<string> GetOrderKey(Guid orderId)
         {
             var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
             return orgId.ToString() + _currentUser.UserId + orderId;
         }

         private async Task<string> GetLookupKey()
         {
             var orgId = await _currentOrganization.GetCurrentOrganizationIdAsync();
             return orgId.ToString() + _currentUser.UserId + CACHE_KEY;
         }
     }
}