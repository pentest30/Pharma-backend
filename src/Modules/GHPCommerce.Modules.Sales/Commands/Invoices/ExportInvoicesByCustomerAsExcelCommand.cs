using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.DTOs;
using GHPCommerce.Core.Shared.Contracts.Tiers.Customers.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.DTOs.Invoices;
using GHPCommerce.Modules.Sales.Queries.Invoices;
using OfficeOpenXml;

namespace GHPCommerce.Modules.Sales.Commands.Invoices
{
    // exportation excel journal des ventes par facture
    public class ExportInvoicesByCustomerAsExcelCommand : ICommand<string>
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public Guid CustomerId { get; set; }
    }
// exportation excel  journal des ventes par produit
    public class ExportInvoicesItemsByCustomerAsExcelCommand : ICommand<string>
    {
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public Guid? CustomerId { get; set; }
    }

    public class ExportInvoicesItemsByCustomerAsExcelCommandHandler : ICommandHandler<ExportInvoicesItemsByCustomerAsExcelCommand, string>
    {
        private readonly ICommandBus _commandBus;

        public ExportInvoicesItemsByCustomerAsExcelCommandHandler(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public async Task<string> Handle(ExportInvoicesItemsByCustomerAsExcelCommand request, CancellationToken cancellationToken)
        {
            var products = await _commandBus.SendAsync(new GetAllProductsByCustomerForSalesPersonQueryV1 { CustomerId = request.CustomerId , Start = request.Start, End = request.End}, cancellationToken);
            var customer = new CustomerDtoV1();
            if (request.CustomerId.HasValue)
            {
                customer = await _commandBus.SendAsync(new GetCustomerByIdQuery { Id = request.CustomerId.Value },cancellationToken);
            }
            var strFilePath = Path.GetTempPath();
            string excelName = $"journal-des-ventes-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(string.Concat(strFilePath, excelName))))
            {
                
                ExcelWorksheet sl = package.Workbook.Worksheets.Add("journal des ventes");
                var e = 5;
                sl.Cells.Style.Font.Size = 14;
                if (request.CustomerId.HasValue)
                {
                    sl.Cells[3 , 1].Value = $"Client: {customer.Name}";
                }
                if (request.Start.HasValue)
                {
                    sl.Cells[3 , 2].Value = $"DU: {request.Start.Value.ToShortDateString()}";
                    if (request.End.HasValue)
                        sl.Cells[3, 2].Value += $" AU: {request.End.Value.ToShortDateString()}";
                    else
                    {
                        sl.Cells[3, 2].Value += $" AU: {DateTime.Now.ToShortDateString()}";
                    }
                }
                sl.Cells[e , 1].Value = "Code article";
                sl.Cells[e, 2].Value = "Désignation";
                sl.Cells[e, 3].Value = "Quantité";
                sl.Cells[e, 4].Value = "Total HT";
                sl.Cells[e, 5].Value = "Total TTC";
                sl.Cells[e, 6].Value = "Laboratoire";
                sl.Cells[e , 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                foreach (var item in products.OrderByDescending(x=>x.TotalTTC))
                {
                    sl.Cells[e + 1 , 1].Value = item.ProductCode;
                    sl.Cells[e + 1 , 2].Value = item.ProductName;
                    sl.Cells[e + 1 , 3].Value = item.Quantity;
                    sl.Cells[e + 1 , 4].Value = item.TotalHT;
                    sl.Cells[e + 1 , 5].Value = item.TotalTTC;
                    sl.Cells[e + 1 , 6].Value = item.Manufacturer;
                    //j++;
                    e++;
                }

                for (var i = 1; i <= sl.Dimension.End.Column; i++)
                {
                    sl.Column(i).AutoFit();
                }

                await package.SaveAsync(cancellationToken);
                return string.Concat(strFilePath, excelName);
            }
        }
    }

    public  class  ExportInvoicesByCustomerAsExcelCommandHandler : ICommandHandler<ExportInvoicesByCustomerAsExcelCommand, string>
    {
        private readonly ICommandBus _commandBus;

        public ExportInvoicesByCustomerAsExcelCommandHandler(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }

        public async Task<string> Handle(ExportInvoicesByCustomerAsExcelCommand request, CancellationToken cancellationToken)
        {
            var invoices = await _commandBus.SendAsync(new GetAllInvoicesForSalesPersonByCustomerQueryV1 { CustomerId = request.CustomerId , Start = request.Start, End = request.End}, cancellationToken);
            var strFilePath = Path.GetTempPath();
            string excelName = $"journal-des-ventes-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage(new FileInfo(string.Concat(strFilePath, excelName))))
            {
                
                ExcelWorksheet sl = package.Workbook.Worksheets.Add("journal des ventes");
                var e = 5;
                sl.Cells.Style.Font.Size = 14;
                sl.Cells[3 , 1].Value = $"Client: {invoices.FirstOrDefault()?.CustomerName}";
                SetPeriod(request, sl, invoices);
                sl.Cells[e , 2].Value = "N° de facture";
                sl.Cells[e, 3].Value = "Date de facturation";
                sl.Cells[e, 4].Value = "Total HT";
                sl.Cells[e, 5].Value = "Total TTC";
                sl.Cells[e, 6].Value = "Total Tax";
                sl.Cells[e, 7].Value = "Total des remises";
                sl.Cells[e , 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 3].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 5].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 6].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                sl.Cells[e , 7].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                sl.Cells[e , 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                foreach (var item in invoices.OrderByDescending(x=>x.InvoiceDate))
                {
                    sl.Cells[e + 1 , 2].Value = item.SequenceNumber;
                    sl.Cells[e + 1 , 3].Value = item.InvoiceDate.ToShortDateString();
                    sl.Cells[e + 1 , 4].Value = item.TotalHT;
                    sl.Cells[e + 1 , 5].Value = item.TotalTTC;
                    sl.Cells[e + 1 , 6].Value = item.TotalTax;
                    sl.Cells[e + 1 , 7].Value = item.TotalDisplayedDiscount;
                    //j++;
                    e++;
                }

                for (var i = 1; i <= sl.Dimension.End.Column; i++)
                {
                    sl.Column(i).AutoFit();
                }

                await package.SaveAsync(cancellationToken);
                return string.Concat(strFilePath, excelName);
            }
        }

        private static void SetPeriod(ExportInvoicesByCustomerAsExcelCommand request, ExcelWorksheet sl, List<InvoiceDtoV1> invoices)
        {
            if (request.Start.HasValue)
            {
                sl.Cells[3, 2].Value = $"DU: {request.Start.Value.ToShortDateString()}";
                if (request.End.HasValue)
                    sl.Cells[3, 2].Value += $" AU: {request.End.Value.ToShortDateString()}";
                else
                {
                    sl.Cells[3, 2].Value += $" AU: {DateTime.Now.ToShortDateString()}";
                }
            }
            else
            {
                sl.Cells[3, 2].Value =
                    $"DU: {invoices.OrderBy(x => x.InvoiceDate).FirstOrDefault()?.InvoiceDate.ToShortDateString()}";
                if (request.End.HasValue)
                    sl.Cells[3, 2].Value += $" AU: {request.End.Value.ToShortDateString()}";
                else
                {
                    sl.Cells[3, 2].Value += $" AU: {DateTime.Now.ToShortDateString()}";
                }
            }
        }
    }
}