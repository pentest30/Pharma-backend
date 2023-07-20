using AutoMapper;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Sectors.Commands
{
    public class CreateSectorCommand : ICommand<ValidationResult>
    {
        public int ExternalId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }
    public class CreateSectorCommandConfigMapping : Profile
    {
        public CreateSectorCommandConfigMapping()
        {
            CreateMap<SectorCustomer, CreateSectorCommand>()
                .ReverseMap();

        }
    }
}
