using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;
using GHPCommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;

namespace GHPCommerce.Application.Catalog.INNCodes.Queries
{
    public class InnCodeQueriesHandler :
        ICommandHandler<GetInnCodesListQuery, PagingResult<InnCodeDto>>,
        ICommandHandler<GetInnCodeByIdQuery, InnCodeDto>,
        ICommandHandler<GetAllInnCodesQuery, IEnumerable<InnCodeDto >>

    {
        private readonly IRepository<INNCode, Guid> _innCodeRepository;
        private readonly IRepository<Form, Guid> _formRepository;
        private readonly IRepository<INN, Guid> _innRepository;
        private readonly IRepository<Dosage, Guid> _dosageRepository;

        public InnCodeQueriesHandler(IRepository<INNCode, Guid> innCodeRepository, 
            IRepository<Form, Guid> formRepository,
            IRepository<INN, Guid> innRepository,
            IRepository<Dosage, Guid> dosageRepository)
        {
            _innCodeRepository = innCodeRepository;
            _formRepository = formRepository;
            _innRepository = innRepository;
            _dosageRepository = dosageRepository;
        }
        public async Task<PagingResult<InnCodeDto>> Handle(GetInnCodesListQuery request, CancellationToken cancellationToken)
        {
            var total = await _innCodeRepository.Table.CountAsync(cancellationToken);
            string orderQuery = string.IsNullOrEmpty(request.SortProp) ? "Name " + request.SortDir : "Form.Name " + request.SortDir;

            var query = await (from inn in _innCodeRepository
                    .Table
                    .Paged(request.Page, request.PageSize)
                    .Include(x=>x.InnCodeDosages)
                    .Include(x=>x.Form)
                    .OrderBy(orderQuery)
               
                select new InnCodeDto
                {
                    Id= inn.Id,
                    Name = inn.Name,
                    FormName = inn.Form.Name,
                    FormId = inn.Form.Id,
                    InnCodeDosages =  inn.InnCodeDosages
                }).ToListAsync(cancellationToken);
            return new PagingResult<InnCodeDto> { Data = query, Total = total };
        }

        public async Task<InnCodeDto> Handle(GetInnCodeByIdQuery request, CancellationToken cancellationToken)
        {
            var innExist = await _innCodeRepository.Table.AnyAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (!innExist)
                throw new NotFoundException($"INN Code with id: {request.Id} was not found");
            var query = await (from inn in _innCodeRepository.Table
                where inn.Id == request.Id
                select new InnCodeDto
                {
                    Id = inn.Id,
                    Name = inn.Name,
                    FormName = inn.Form.Name,
                    FormId = inn.Form.Id,
                    InnCodeDosages = inn.InnCodeDosages
                }).FirstOrDefaultAsync(cancellationToken: cancellationToken);
            return query;
        }

        public async Task<IEnumerable<InnCodeDto>> Handle(GetAllInnCodesQuery request, CancellationToken cancellationToken)
        {
            var frms =await _formRepository.Table.ToListAsync(cancellationToken: cancellationToken);
            var codes = await _innCodeRepository.Table.ToListAsync(cancellationToken: cancellationToken);
            var query =  (from inn in codes
               join frm in frms on inn.FormId equals frm.Id
                select new {frm, inn, inn.InnCodeDosages}).ToList();
           
            var list = new List<InnCodeDto>();
            foreach (var item in query.GroupBy(x=>x.inn))
            {
                var from = query.FirstOrDefault(x => x.inn.Id == item.Key.Id);
                var innCode =new InnCodeDto(item.Key.Id,item.Key.Name, from?.frm.Name, from.frm.Id );
               
                innCode.InnCodeDosages = new List<INNCodeDosage>();
                foreach (var r in item)
                {
                    var innDosageCode = r.InnCodeDosages.FirstOrDefault();
                    if (innDosageCode != null)
                    {
                        innDosageCode.Inn = new INN {Name = innDosageCode.Inn.Name};
                        innDosageCode.Dosage = new Dosage {Name = innDosageCode.Dosage.Name};
                        innDosageCode.InnCode = null;
                        innCode.InnCodeDosages.Add(innDosageCode);
                    }
                }
                list.Add(innCode);
            }
            return list;
        }
    }
}
