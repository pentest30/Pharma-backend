using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Inventory.DTOs.TransferLogs;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Queries.TransferLogs
{
    public class GetTransferLogByIdQuery : ICommand<TransferLogDto>
    {
        public Guid Id { get; set; }
    }
    public class  GetTransferLogByIdQueryHandler : ICommandHandler<GetTransferLogByIdQuery, TransferLogDto>
    {
        private readonly IRepository<TransferLog, Guid> _repository;
        private readonly IMapper _mapper;

        public GetTransferLogByIdQueryHandler(IRepository<TransferLog, Guid> _repository, IMapper mapper)
        {
            this._repository = _repository;
            _mapper = mapper;
        }
        public async Task<TransferLogDto> Handle(GetTransferLogByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _repository.Table
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            return _mapper.Map<TransferLogDto>(item);
        }
    }
}