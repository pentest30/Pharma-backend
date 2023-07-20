using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Shared;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Infra.OS;

namespace GHPCommerce.Application.Catalog.RequestLogs
{
    public class RequestLogCommandHandler :ICommandHandler<LogApiModel, object>
    {
        private readonly IRepository<LogRequest, Guid> _repository;
        private readonly IMapper _mapper;

        public RequestLogCommandHandler(IRepository<LogRequest, Guid> repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<object> Handle(LogApiModel request, CancellationToken cancellationToken)
        {
            await LockProvider<string>.WaitAsync(nameof(LogApiModel), cancellationToken);
            var log = _mapper.Map<LogRequest>(request);
            _repository.Add(log);
            await _repository.UnitOfWork.SaveChangesAsync();
            LockProvider<string>.Release(nameof(LogApiModel));
            return default;
        }
    }
}