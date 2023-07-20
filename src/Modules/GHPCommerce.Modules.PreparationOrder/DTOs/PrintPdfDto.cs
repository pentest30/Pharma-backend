namespace GHPCommerce.Modules.PreparationOrder.DTOs
{
    public class PrintPdfDto
    {
        public byte[] Data { get; set; }
        public int TotalPages { get; set; }
        public string ErrorMessage { get; set; }
    }
}