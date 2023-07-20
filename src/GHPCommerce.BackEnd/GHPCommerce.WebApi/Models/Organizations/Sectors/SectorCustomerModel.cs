using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using GHPCommerce.Application.Tiers.Sectors.Commands;
using GHPCommerce.Application.Tiers.Sectors.Queries;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.WebApi.Models.Organizations.Sectors
{
    public class SectorCustomerModel
    {
      
        public string Name { get; set; }
        public string Code { get; set; }
        public int ExternalId { get; set; }
        
    }
    public class SectorCustomerModelConfigMapping : Profile
    {
        public SectorCustomerModelConfigMapping()
        {
            CreateMap<SectorCustomerModel, CreateSectorCommand>()
                .ReverseMap();
            CreateMap<SectorCustomerModel, UpdateSectorCommand>()
                .ReverseMap();

        }
    }

    public class SectorCustomerModelValidator : AbstractValidator<SectorCustomerModel>
    {
        private readonly ICommandBus _commandBus;

        public SectorCustomerModelValidator(ICommandBus commandBus)
        {
            _commandBus = commandBus;
            RuleFor(v => v.Name)
                .MaximumLength(200).NotEmpty()
                .MustAsync(NameMustBeUnique)
                .WithMessage("Le Nom doit être unique");
            RuleFor(v => v.Code)
                .MaximumLength(200)
                .NotEmpty();
           
        }

        private async Task<bool> NameMustBeUnique(SectorCustomerModel arg1, string arg2, CancellationToken arg3)
        {
            var unique = await _commandBus.SendAsync(new CheckSectorByNameQuery {Code = arg1.Code, Name = arg2}, arg3);
            return !unique;
        }
    }
}
