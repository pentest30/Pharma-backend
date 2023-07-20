using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.INNs.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
namespace GHPCommerce.Application.Catalog.INNs.Queries
{
    public class InnQueriesHandler :
        ICommandHandler<GetINNsListQuery, PagingResult<InnDto>>,
        ICommandHandler<GetInnByIdQuery, InnDto>,
        ICommandHandler<GetAllInnsQuery , IEnumerable<InnDto>>

    {
        private readonly IRepository<INN, Guid> _innRepository;
        private readonly IMapper _mapper;

        public InnQueriesHandler(IRepository<INN, Guid> innRepository, IMapper mapper)
        {
            _innRepository = innRepository;
            _mapper = mapper;
        }
        public async Task<PagingResult<InnDto>> Handle(GetINNsListQuery request, CancellationToken cancellationToken)
        {
            var total = await _innRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _innRepository
                .Table
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data =  _mapper.Map<IEnumerable<InnDto>>(query);
            return new PagingResult<InnDto> { Data = data, Total = total };
        }

        public async Task<InnDto> Handle(GetInnByIdQuery request, CancellationToken cancellationToken)
        {
            var form = await _innRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);

            if (form == null)
                throw new NotFoundException($"Inn with id: {request.Id} was not found");
            return _mapper.Map<InnDto>(form);
        }

        public async Task<IEnumerable<InnDto>> Handle(GetAllInnsQuery request, CancellationToken cancellationToken)
        {
            var query = await _innRepository
                .Table
               .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<InnDto>>(query);
        }
    }
}
