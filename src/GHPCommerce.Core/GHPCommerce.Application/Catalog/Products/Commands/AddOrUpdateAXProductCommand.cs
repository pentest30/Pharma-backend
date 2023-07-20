using FluentValidation;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Products.Commands
{
    public class AddOrUpdateAXProductCommand : ICommand<ValidationResult>
    {
       
        public string Code { get; set; }
        public string FullName { get; set; }

        public bool Status { get; set; }
        public bool Princeps { get; set; }
        public string InnCode { get; set; }

        public string DciConcat { get; set; }
        public string TaxGroupCode { get; set; }

        public string ManufacturerCode { get; set; }
        public string ProductGroupCode { get; set; }
        public bool Psychotropic { get; set; }
        //public string ProductClassCode { get; set; }
        public bool Thermolabile { get; set; }
        public string FormCode { get; set; }
        public string DefaultLocation { get; set; }
        public string PikingZone { get; set; }
    }
    public class CreateAXProductCommandValidator : AbstractValidator<AddOrUpdateAXProductCommand>
    {
        public CreateAXProductCommandValidator()
        {
            RuleFor(v => v.FullName)
                //.MaximumLength(200).
                .NotEmpty();
            RuleFor(v => v.Code)
               // .MaximumLength(200)
                .NotEmpty();
            // RuleFor(v => v.ManufacturerCode)
            //     .NotEmpty(); 
                //.Must(x => !x.Equals(default));
                RuleFor(v => v.TaxGroupCode)
                    .NotEmpty();
        }
    }
}
