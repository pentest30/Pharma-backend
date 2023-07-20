using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Core.Shared.Contracts.Batches.Commands;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Modules.Inventory.Entities;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Inventory.Commands.Batches
{
    public class BatchesCommandsHandler : ICommandHandler<CreateBatchCommand, Tuple<Guid, ValidationResult> >, ICommandHandler<DeleteBatchCommand, ValidationResult>
    {
        private readonly IRepository<Batch, Guid> _batchRepository;
        private readonly IMapper _mapper;

        public BatchesCommandsHandler(IRepository<Batch, Guid> batchRepository, IMapper mapper)
        {
            _batchRepository = batchRepository;
            _mapper = mapper;
        }
        public  async Task<Tuple<Guid, ValidationResult>> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateBatchCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
            if (!validationErrors.IsValid)
                return new Tuple<Guid, ValidationResult>(Guid.Empty, validationErrors);
            try
            {
                var batch = _mapper.Map<Batch>(request);
                _batchRepository.Add(batch);
                await _batchRepository.UnitOfWork.SaveChangesAsync();
                return new Tuple<Guid, ValidationResult>(batch.Id, validationErrors);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        public async Task<ValidationResult> Handle(DeleteBatchCommand request, CancellationToken cancellationToken)
        {
            var batch = await _batchRepository.Table.FirstOrDefaultAsync(x => x.Id == request.BatchId, cancellationToken: cancellationToken);
            if(batch==null)         return new ValidationResult()
                { Errors = { new ValidationFailure("ReservationError", "Batch not found") } };
            _batchRepository.Delete(batch);
            await _batchRepository.UnitOfWork.SaveChangesAsync();
            return default;

        }
    }
}