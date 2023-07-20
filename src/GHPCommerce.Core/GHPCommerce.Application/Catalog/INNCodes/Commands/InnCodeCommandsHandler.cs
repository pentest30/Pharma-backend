using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.CrossCuttingConcerns.Exceptions;
using GHPCommerce.Domain.Domain.Catalog;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using  System.Linq;
using AutoMapper;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;

namespace GHPCommerce.Application.Catalog.INNCodes.Commands
{
    public class InnCodeCommandsHandler :
        ICommandHandler<CreateInnCodeCommand, ValidationResult>,
        ICommandHandler<UpdateInnCodeCommand, ValidationResult>,
        ICommandHandler<CreateInnCodeLineCommand>,
        ICommandHandler<DeleteInnCodeCommand>
    {
        private readonly IRepository<INNCode, Guid> _innCodeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public InnCodeCommandsHandler(IRepository<INNCode, Guid> innCodeRepository, IMapper mapper)
        {
            _innCodeRepository = innCodeRepository;
            _mapper = mapper;
            _unitOfWork = innCodeRepository.UnitOfWork;
        }
        public async Task<ValidationResult> Handle(CreateInnCodeCommand request, CancellationToken cancellationToken)
        {
            var validator = new CreateInnCodeCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            await ValidateName(request.Id, request.Name, cancellationToken, validationErrors);
            CheckDuplicateItems(request.InnCodeDosages, validationErrors);
            if (!validationErrors.IsValid)
                return validationErrors;
            var innCode = new INNCode(request.Name, request.FormId);
            innCode.Id = request.Id;
            innCode.InnCodeDosages = new List <INNCodeDosage>();
            if (request.InnCodeDosages.Any())
                request.InnCodeDosages.ForEach(x =>
                {
                    innCode.AddInnCodeLine(request.Id, x.INNId, x.DosageId);
                });

            _innCodeRepository.Add(innCode);

            await _unitOfWork.SaveChangesAsync();
            return default;
        }



        public async Task<ValidationResult> Handle(UpdateInnCodeCommand request, CancellationToken cancellationToken)
        {
            var innCode = await _innCodeRepository
                .Table
                .Include(x=>x.InnCodeDosages)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (innCode == null)
                throw new NotFoundException($"INN Code with id: {request.Id} was not found");
            var validator = new UpdateInnCodeCommandValidator();
            var validationErrors = await validator.ValidateAsync(request, cancellationToken);
            CheckDuplicateItems(request.InnCodeDosages, validationErrors);

            if (!validationErrors.IsValid)
                return validationErrors;
            innCode.UpdateInnCode(request.Name, request.FormId, _mapper.Map<List<INNCodeDosage>>(request.InnCodeDosages));
            await _unitOfWork.SaveChangesAsync();
            return default;
        }

        public async Task<Unit> Handle(DeleteInnCodeCommand request, CancellationToken cancellationToken)
        {
            var inn = await _innCodeRepository.Table.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken: cancellationToken);
            if (inn == null)
                throw new NotFoundException($"INN Code with id: {request.Id} was not found");
            _innCodeRepository.Delete(inn);
            await _unitOfWork.SaveChangesAsync();
            return Unit.Value;
        }

        public async Task<Unit> Handle(CreateInnCodeLineCommand request, CancellationToken cancellationToken)
        {
            var inn = await _innCodeRepository.Table.FirstOrDefaultAsync(x => x.Id == request.InnCodeId, cancellationToken: cancellationToken);
            if (inn == null)
                throw new NotFoundException($"INN Code with id: {request.Id} was not found");
            inn.AddInnCodeLine(request.InnCodeId, request.InnCodeDosageDto.INNId, request.InnCodeDosageDto.DosageId);
            _innCodeRepository.Update(inn);
            await _unitOfWork.SaveChangesAsync();
            return default;

        }
        private async Task ValidateName(Guid id, string name, CancellationToken cancellationToken,
            ValidationResult validationErrors)
        {
            var existingName =
                await _innCodeRepository.Table.AnyAsync(x => x.Name == name && x.Id != id,
                    cancellationToken: cancellationToken);
            if (existingName)
                validationErrors.Errors.Add(new ValidationFailure("Code",
                    "There is an INN code with the same  name, please change the  name "));
        }
        private static void CheckDuplicateItems(List<InnCodeDosageDto> innCodeDosages, ValidationResult validationErrors)
        {
            var dup = innCodeDosages
                .GroupBy(x => new { x.INNId})
                .Select(group => new { Name = @group.Key, Count = @group.Count() });
            foreach (var item in dup)
            {
                if (item.Count <= 1) continue;
                validationErrors.Errors.Add(new ValidationFailure("Duplication",
                    "Il y a une ou plusieurs lignes en double"));
                break;
            }
        }
    }
}
