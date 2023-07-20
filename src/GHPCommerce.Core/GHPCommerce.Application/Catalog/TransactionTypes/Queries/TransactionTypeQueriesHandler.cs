using AutoMapper;
using GHPCommerce.Application.Catalog.TransactionTypes.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.DTOs;
using GHPCommerce.Core.Shared.Contracts.Catalog.Queries;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHPCommerce.Application.Catalog.TransactionTypes.Queries
{
    public class TransactionTypeQueriesHandler :
        ICommandHandler<GetPagedTransactionTypeQuery, SyncPagedResult<TransactionTypeDto>>,
        ICommandHandler<GetAllTransactionTypeQuery, IEnumerable<TransactionTypeDto>>,
        ICommandHandler<GetTransactionTypeByCode, TransactionTypeDtoV1>
    {
        private readonly IRepository<TransactionType, Guid> _transactionTypeRepository;
        private readonly IMapper _mapper;

        public TransactionTypeQueriesHandler(IRepository<TransactionType, Guid> transactionTypeRepository, IMapper mapper)
        {
            _transactionTypeRepository = transactionTypeRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<TransactionTypeDto>> Handle(GetAllTransactionTypeQuery request, CancellationToken cancellationToken)
        {
            var query = await _transactionTypeRepository
                        .Table
                        .OrderBy(x => x.TransactionTypeName)
                        .ToListAsync(cancellationToken);
            var result = _mapper.Map<List<TransactionTypeDto>>(query);

            return result;
        }

        public async Task<SyncPagedResult<TransactionTypeDto>> Handle(GetPagedTransactionTypeQuery request, CancellationToken cancellationToken)
        {
            var query = _transactionTypeRepository.Table;
            var result = await query
                 .OrderByDescending(x => x.TransactionTypeName)
                 .ToListAsync(cancellationToken: cancellationToken);
            var r = result
               .Skip(request.DataGridQuery.Skip / request.DataGridQuery.Take)
               .Take(request.DataGridQuery.Take);
            return new SyncPagedResult<TransactionTypeDto>
            {
                Count = result.Count(),
                Result = _mapper.Map<List<TransactionTypeDto>>(r)
            };
        }

        public async Task<TransactionTypeDtoV1> Handle(GetTransactionTypeByCode request, CancellationToken cancellationToken)
        {
            var transactionType = await _transactionTypeRepository.Table.Where(c => c.CodeTransaction == request.Code).FirstOrDefaultAsync(cancellationToken);
            return _mapper.Map<TransactionTypeDtoV1>(transactionType);

        }
    }
}
