namespace GHPCommerce.Infra.OS.Print
{
    public class PrinterOptions
    {
        public int DefaultPrinter { get; set; }
        public string[] Printers { get; set; }
        public string[] RawPrinters { get; set; }
    }
}