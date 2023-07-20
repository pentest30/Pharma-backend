using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Application.Catalog.Forms.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
namespace GHPCommerce.Application.Catalog.Forms.Queries
{
    public class FormsQueriesHandler :
        ICommandHandler<GetFormsListQuery, PagingResult<FormDto>>,
        ICommandHandler<GetFormByIdQuery, FormDto>,
        ICommandHandler<GetAllFormsQuery, IEnumerable<FormDto>>
    {
        private readonly IRepository<Form, Guid> _formRepository;
        private readonly IMapper _mapper;

        public FormsQueriesHandler(IRepository<Form, Guid> formRepository, IMapper mapper)
        {
            _formRepository = formRepository;
            _mapper = mapper;
        }
        public async Task<PagingResult<FormDto>> Handle(GetFormsListQuery request, CancellationToken cancellationToken)
        {
            var total = await _formRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : request.SortProp + " " + request.SortDir;

            var query = await _formRepository
                .Table.OrderBy(orderQuery)
                .Paged(request.Page, request.PageSize)
                .ToListAsync(cancellationToken: cancellationToken);
            var data= _mapper.Map<IEnumerable<FormDto>>(query);
            return new PagingResult<FormDto> { Data = data, Total = total };
        }

        public async Task<FormDto> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
        {
            var form = await _formRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);

            if (form == null)
                throw new NotFoundException($"form with id: {request.Id} was not found");
            return _mapper.Map<FormDto>(form);
        }

        public async Task<IEnumerable<FormDto>> Handle(GetAllFormsQuery request, CancellationToken cancellationToken)
        {
            var query = await _formRepository
                .Table
                
                .ToListAsync(cancellationToken: cancellationToken);
           return _mapper.Map<IEnumerable<FormDto>>(query);
        }
    }
}
