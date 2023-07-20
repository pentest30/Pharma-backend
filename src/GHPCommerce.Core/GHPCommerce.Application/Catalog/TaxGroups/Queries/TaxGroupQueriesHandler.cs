using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.TaxGroups.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.TaxGroups.Queries
{
    public class TaxGroupQueriesHandler :
        ICommandHandler<GetTaxGroupsListQuery, PagingResult<TaxGroupDto>>,
        ICommandHandler<GetTaxGroupsByIdQuery, TaxGroupDto>,
        ICommandHandler<GetAllTaxesQuery, IEnumerable<TaxGroupDto>>
    {
        private readonly IRepository<TaxGroup, Guid> _taxGroupRepository;
        private readonly IMapper _mapper;

        public TaxGroupQueriesHandler(IRepository<TaxGroup, Guid> taxGroupRepository, IMapper mapper)
        {
            _taxGroupRepository = taxGroupRepository;
            _mapper = mapper;
        }
        public async Task<PagingResult<TaxGroupDto>> Handle(GetTaxGroupsListQuery request, CancellationToken cancellationToken)
        {
            var total = await _taxGroupRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _taxGroupRepository
                .Table
                .OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);

            var data =  _mapper.Map<IEnumerable<TaxGroupDto>>(query);
            return new PagingResult<TaxGroupDto> { Data = data, Total = total };
        }

        public async Task<TaxGroupDto> Handle(GetTaxGroupsByIdQuery request, CancellationToken cancellationToken)
        {
            var taxGroup = await _taxGroupRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
            if (taxGroup == null)
                throw new NotFoundException($"Tax group with id: {request.Id} was not found");
            return _mapper.Map<TaxGroupDto>(taxGroup);
        }

        public async Task<IEnumerable<TaxGroupDto>> Handle(GetAllTaxesQuery request, CancellationToken cancellationToken)
        {
            var query = await _taxGroupRepository
                .Table
                .OrderBy(x=>x.TaxValue)
                .Select(x => new TaxGroupDto {Id = x.Id, Name = x.Code})
                .ToListAsync(cancellationToken);
            return query;
        }
    }
}
