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

namespace GHPCommerce.Application.Catalog.Lists.Commands
{
    public class ListCommandsHandler :
        ICommandHandler<CreateListCommand, ValidationResult>,
        ICommandHandler<UpdateLisCommand , ValidationResult>,
        ICommandHandler<DeleteListCommand>
    {
        private readonly IRepository<List, Guid> _listRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ListCommandsHandler(IRepository<List, Guid> listRepository)
        {
            _listRepository = listRepository;
            _unitOfWork = listRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreateListCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateListCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, request.Code, cancellationToken, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            var list = new List(request.Id, request.Code,request.Name, request.SHP);
            _listRepository.Add(list);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<ValidationResult> Handle(UpdateLisCommand request, CancellationToken cancellationToken)
        {
            var list = await _listRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id,
                cancellationToken: cancellationToken);
            if (list == null)
                throw new NotFoundException($"list with id: {request.Id} was not found");
            var validator = new CreateListCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, request.Code, cancellationToken, validationErrors);

            if (!validationErrors.IsValid)
                return validationErrors;
            list.Code = request.Code;
            list.Name = request.Name;
            list.SHP = request.SHP;
            _listRepository.Update(list);
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteListCommand request, CancellationToken cancellationToken)
        {
            var list = await _listRepository
                .Table
                .FirstOrDefaultAsync(x => x.Id == request.Id,
                    cancellationToken: cancellationToken);
            
            if (list == null)
                throw new NotFoundException($"list with id: {request.Id} was not found");
          
            _listRepository.Delete(list);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }
        private async Task ValidateName(Guid id, string name, string code, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var existingName =
                await _listRepository.Table.AnyAsync(x => x.Name == name && x.Id !=id,
                    cancellationToken: cancellationToken);
            if (existingName )
                validationErrors.Errors.Add(new ValidationFailure("name",
                    "There is a list with the same  name, please change the  name "));
            var existingCode =
                await _listRepository.Table.AnyAsync(x => x.Code == code && x.Id != id,
                    cancellationToken: cancellationToken);
            if (existingCode)
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is a list with the same  code, please change the  code "));
        }
    }
}
