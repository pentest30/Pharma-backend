using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Tiers;

namespace GHPCommerce.Application.Tiers.Organizations.Commands
{
    public class CreateAXOrganizationCommand : ICommand<ValidationResult>
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string NIS { get; set; }
        public string NIF { get; set; }
        public string RC { get; set; }
        public string AI { get; set; }
        public string OrganizationActivity { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string DefaultSalesPerson { get; set; }
        public string DefaultSalesGroup { get; set; }
        public string DefaultDeliverySector { get; set; }
        public decimal Dept { get; set; }
        public decimal LimitCredit { get; set; }
        public int PaymentDeadline { get; set; }
        public CustomerState CustomerState { get; set; }
        public string CustomerGroup { get; set; }
        public decimal MonthlyObjective { get; set; }
        public string PaymentMethod { get; set; }
        public string OrganizationGroupCode { get; set; }
        public string ZipCode { get; set; }


    }
    public class CreateAXOrganizationCommandConfigMapping : Profile
    {
        public CreateAXOrganizationCommandConfigMapping()
        {
            CreateMap< Organization, CreateAXOrganizationCommand > ().ReverseMap()
                .ForMember(x => x.Activity, o => o.Ignore())
                .ForMember(x => x.OrganizationGroupCode, o => o.Ignore());
            //.ForMember(x=>x.);

        }
    }
    public class CreateAXOrganizationCommandValidator : AbstractValidator<CreateAXOrganizationCommand>
    {
      

        public CreateAXOrganizationCommandValidator()
        {
            RuleFor(v => v.Name)
               // .MaximumLength(200)
                .NotEmpty();
            RuleFor(v => v.Code).NotEmpty();
           // RuleFor(v => v.OrganizationGroupCode).NotEmpty();

        }

        
    }
}
