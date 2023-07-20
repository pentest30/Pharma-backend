using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Application.Catalog.Forms.Commands
{
    public class FormCommandsHandler : ICommandHandler<CreateFormCommand , ValidationResult>,
        ICommandHandler< UpdateFormCommand, ValidationResult>,
        ICommandHandler<DeleteFormCommand>

    {
        private readonly IRepository<Form, Guid> _formRepository;
        private readonly IRepository<INNCode, Guid> _innCodeRepository;
        private readonly IUnitOfWork _unitOfWork;
        public FormCommandsHandler(IRepository<Form, Guid> formRepository, IRepository<INNCode, Guid> innCodeRepository)
        {
            _formRepository = formRepository;
            _innCodeRepository = innCodeRepository;
            _unitOfWork = formRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreateFormCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateFormCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request, cancellationToken, validationErrors);

            if (!validationErrors.IsValid)
                return validationErrors;
            var form = new Form(request.Id, request.Name);
            _formRepository.Add(form);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }

        private async Task ValidateName(CreateFormCommand request, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var existingForm =
                await _formRepository.Table.AnyAsync(x => x.Name == request.Name,
                    cancellationToken: cancellationToken);
            if (existingForm )
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is a form with the same  name, please change the  name "));
        }

        public async Task<ValidationResult> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdateFormCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request, cancellationToken, validationErrors);

            if (!validationErrors.IsValid)
                return validationErrors;

            var form = await _formRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                cancellationToken: cancellationToken);
            if (form == null)
                throw new NotFoundException($"Form with id: {request.Id} was not found");
            form.Name = request.Name.ToUpper();
            _formRepository.Update(form);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
        {
            var form = await _formRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id,
                cancellationToken: cancellationToken);
            if (form == null)
                throw new NotFoundException($"Form with id: {request.Id} was not found");
            var hasInnCodes =
                await _innCodeRepository
                    .Table
                    .AnyAsync(x => x.FormId == request.Id,
                        cancellationToken: cancellationToken);
            if (hasInnCodes)
                throw new InvalidOperationException("You cannot remove this Item");
            _formRepository.Delete(form);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
    }
}
