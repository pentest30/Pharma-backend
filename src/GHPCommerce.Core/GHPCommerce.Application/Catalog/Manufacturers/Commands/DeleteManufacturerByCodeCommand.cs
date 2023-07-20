namespace GHPCommerce.Application.Catalog.Manufacturers.Commands
{
    public class DeleteManufacturerByCodeCommand : DeleteManufacturerCommand
    {
        public string Code { get; set; }
    }
}
