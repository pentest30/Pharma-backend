using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Lists.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace GHPCommerce.Application.Catalog.Lists.Queries
{
    public class ListQueriesHandler : 
        ICommandHandler<GetListsListQuery , PagingResult<ListDto>>,
        ICommandHandler<GetListByIdQuery, ListDto>,
        ICommandHandler<GetAllListQuery, IEnumerable<ListDto>>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IMapper _mapper;

        public ListQueriesHandler(IRepository<List, Guid> listRepository, IMapper mapper)
        {
            _listRepository = listRepository;
            _mapper = mapper;
        }
        public  async Task<PagingResult<ListDto>> Handle(GetListsListQuery request, CancellationToken cancellationToken)
        {
            var total = await _listRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _listRepository
                .Table
                .OrderBy(orderQuery)
                .ToListAsync(cancellationToken: cancellationToken);
            var data =  _mapper.Map<IEnumerable<ListDto>>(query);
            return new PagingResult<ListDto> { Data = data, Total = total };
        }

        public async Task<ListDto> Handle(GetListByIdQuery request, CancellationToken cancellationToken)
        {
            var list = await _listRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);

            if (list == null)
                throw new NotFoundException($"list with id: {request.Id} was not found");
            return _mapper.Map<ListDto>(list);
        }

        public async Task<IEnumerable<ListDto>> Handle(GetAllListQuery request, CancellationToken cancellationToken)
        {
            var query = await _listRepository
                .Table
               
                .ToListAsync(cancellationToken: cancellationToken);
            return _mapper.Map<IEnumerable<ListDto>>(query);
        }
    }
}
