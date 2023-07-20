using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GHPCommerce.Application.Tiers.Organizations.Queries;
using GHPCommerce.Core.Shared.Contracts.Organization.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Sales.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GHPCommerce.Modules.Sales.Helpers
{
    public class OrderToPdfHelper
    {
        private readonly Order _order;
        private readonly ICommandBus _commandBus;

        public OrderToPdfHelper(Order order, ICommandBus commandBus)
        {
            _order = order;
            _commandBus = commandBus;
        }

        public async Task<string> GenerateInvoicePdfFileAsync()
        {
            Document doc = new Document(PageSize.A4, 5, 5, 40, 20);
            var strFilePath = Path.GetTempPath();
            var customer = await _commandBus.SendAsync(new GetOrganizationByIdQuery { Id = _order.CustomerId });
            var supplier = await _commandBus.SendAsync(new GetOrganizationByIdQuery { Id = _order.SupplierId });
            var sector =
                await _commandBus.SendAsync(new GetCustomerSectorQuery() { OrganizationId = _order.CustomerId, SupplierId = _order.SupplierId});

            var fileName = "Pdf_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".pdf";
            string[] headers = { "N°", "REF","DESIGNATION", "LOT","DDP","QTE","PU HT","PT HT","MRG","PPA HT", "PPA TTC", "SHP", "TVA" };
            float[] widths = new float[] { 5f, 12f, 30f, 10f, 15f, 10f, 10f, 12f, 8f, 12f, 12f, 7f, 7f };
            MemoryStream stream = new MemoryStream();
            PdfWriter.GetInstance(doc, stream).CloseStream = false;
            doc.SetMargins(5, 5, 5, 5); // 0.5 inch margins

            doc.Open();
            #region Entete Commande

            var supplierAddress = supplier.Addresses.FirstOrDefault(c => c.Main);
            Paragraph cp = new Paragraph("MED IJK SPA - Capital social: 77 400 000,00",
                new Font(Font.NORMAL, 12, Font.ITALIC, BaseColor.BLACK));
            Paragraph cp2 = new Paragraph("Distributeur de produits Pharmaceutiques et parapharmaceutiques",  new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            Paragraph  headOffice = new Paragraph($"SIEGE SOCIAL {supplierAddress?.Street} - {supplierAddress?.City}",  new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            Paragraph  supplierNisAiRc = new Paragraph($"NIS: {supplier.NIS} - AI: {supplier.AI} RC: {supplier.RC}",  new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            Paragraph  supplierPhone = new Paragraph($"NIF {supplier.NIF} - TEL: {supplier.PhoneNumbers.FirstOrDefault(x => !x.IsFax)}", new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            supplierPhone.SpacingAfter = 10;
            doc.Add(cp);
            doc.Add(cp2);
            doc.Add(headOffice);
            doc.Add(supplierNisAiRc);
            doc.Add(supplierPhone);
            #endregion

            #region Customer information
            var leftCell = new PdfPCell
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            };
            Paragraph orderNumber =  new Paragraph($"Commande N° {_order.OrderNumberSequence}",new Font(Font.NORMAL, 12, Font.BOLDITALIC, BaseColor.BLACK));
          //  Paragraph salesOrderNumber = new Paragraph($"B_CMDE N° {order.OrderNumberSequence} DU {order.CreatedDateTime.Date.ToShortDateString()} - {_order.TotalPackage} COLIS -  {_order.TotalPackageThermolabile} COLIS FROID",new Font(Font.NORMAL, 12, Font.BOLDITALIC, BaseColor.BLACK));
            leftCell.AddElement(orderNumber);
            var rightCell = new PdfPCell
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            };
            rightCell.AddElement(new Paragraph($"Date: {DateTime.Now.Date.ToShortDateString()}",new Font(Font.NORMAL, 12, Font.BOLDITALIC, BaseColor.BLACK)));
            PdfPTable tableLayout1 = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            tableLayout1.DefaultCell.BorderWidth = 0;
            tableLayout1.AddCell(leftCell);
            tableLayout1.AddCell(rightCell);
            doc.Add(tableLayout1);

            //doc.Add(salesOrderNumber);
            doc.SetMargins(10, 10, 10, 10);
            float[] widthsCustomerTable = new float[] {10f, 50f, 40f};
            doc.Add(new Chunk(""));
            PdfPTable tableLayout = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            tableLayout.DefaultCell.BorderWidth = 0;
            leftCell = new PdfPCell
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            };
            // leftCell.Border = 0;
            if (customer == null)
            {
                leftCell.AddElement(SetPhrase($"RC: ", BaseColor.BLACK));
                leftCell.AddElement(SetPhrase($"NIF : ", BaseColor.BLACK));
                leftCell.AddElement(SetPhrase($"AI: ", BaseColor.BLACK));
                leftCell.AddElement(SetPhrase($"NIS: ", BaseColor.BLACK));
                leftCell.AddElement(SetPhrase($"TEL/FAX : ", BaseColor.BLACK));
            }
            else 
            {
                leftCell.AddElement(SetPhrase($"RC: { customer.RC}", BaseColor.BLACK));
           
                leftCell.AddElement(SetPhrase($"NIF : {customer.NIF}", BaseColor.BLACK));
                leftCell.AddElement(SetPhrase($"AI: {customer.AI}", BaseColor.BLACK));
                leftCell.AddElement(SetPhrase($"NIS: {customer.NIS}", BaseColor.BLACK));
                leftCell.AddElement(SetPhrase($"TEL/FAX : ", BaseColor.BLACK));
            }
           

            tableLayout.AddCell(leftCell);
            rightCell = new PdfPCell
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            };

            var orgCode = customer != null ? customer.OrganizationGroupCode.ToUpper() : "";
            var street = customer != null ? customer.Addresses.FirstOrDefault()?.Street : "";
            var city = customer != null ? customer.Addresses.FirstOrDefault()?.City : "";
            var country = customer != null ? customer.Addresses.FirstOrDefault()?.Country : "";
            //  rightCell.Border = 0;
            rightCell.AddElement(SetPhrase($"Nom et prénom: {_order.CustomerName.ToUpper()}", BaseColor.BLACK));
            rightCell.AddElement(SetPhrase($"Code client: {orgCode}", BaseColor.BLACK));

            rightCell.AddElement(SetPhrase($"Adresse: {street }, {city }, {country}", BaseColor.BLACK));
            rightCell.AddElement(SetPhrase($"N° et Date CDE: { _order.CodeAx}, Le: {_order.OrderDate.ToShortDateString()}", BaseColor.BLACK));
            tableLayout.AddCell(rightCell);
            doc.Add(tableLayout);
            doc.Add(new Chunk("\n"));
            #endregion
            PdfPTable pTable = new PdfPTable(headers.Length);
            pTable.WidthPercentage = 100;
            
            pTable.SetWidths(widths);
            
            foreach (string t in headers)
            {
                var cell = new PdfPCell(new Phrase(t, new Font(Font.NORMAL, 8, Font.BOLD, BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    BackgroundColor = BaseColor.WHITE,
                    Padding = 5,
                    
                };
                pTable.AddCell(cell);
            }

            int i = 0;
            foreach (var orderItem in _order.OrderItems)
            {
                i++;
                pTable.AddCell(Cell("000".Substring(0,3 - i.ToString().Length )+ i.ToString(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.ProductCode.ToUpper(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.ProductName.ToUpper(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.InternalBatchNumber.ToUpper(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.ExpiryDate.HasValue? orderItem.ExpiryDate.Value.ToShortDateString(): "", BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.Quantity.ToString(), BaseColor.BLACK));
                pTable.AddCell(Cell(Math.Round(orderItem.UnitPrice,2).ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(Math.Round(orderItem.TotalExlTax,2).ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(Math.Round((orderItem.Discount + orderItem.ExtraDiscount) * 100,2) .ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(Math.Round(orderItem.PpaHT,2).ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(Math.Round(orderItem.PpaTTC,2).ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(Math.Round(orderItem.PFS,2).ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(Math.Round(orderItem.Tax,2).ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
            }
           
            pTable.PaddingTop = 40;
            doc.Add(pTable);
            #region Details Invoice totals
            PdfPTable detailsInvoiceTable = new PdfPTable(3);
            float[] widthsdetailTable = new float[] { 33f, 33f, 33f};

            detailsInvoiceTable.WidthPercentage = 100;
            detailsInvoiceTable.SetWidths(widthsdetailTable);
            detailsInvoiceTable.SpacingBefore = 15;
            detailsInvoiceTable.SpacingAfter = 15;

            string[] headersDetails = { "Totaux Pharm", "Détail TVA","Totaux Facture"  };
            foreach (var t in headersDetails)
            {
                var cell = new PdfPCell(new Phrase(t, new Font(Font.NORMAL, 8, Font.BOLD, BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    BackgroundColor = BaseColor.WHITE,
                    Padding = 5
                };
                detailsInvoiceTable.AddCell(cell);
            }

            var totalTax = Math.Round(_order.OrderItems.Sum(x => x.UnitPrice * x.Quantity * (decimal)x.Tax), 2);
            detailsInvoiceTable.AddCell(Cell($"PPA: {_order.OrderItems.Sum(c => c.PpaTTC)}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TOTAL: {_order.OrderItems.Sum(x => x.UnitPrice * x.Quantity)}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"MARGE: ", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell("", BaseColor.BLACK));
            var discounts = _order.OrderItems.Sum(x => x.UnitPrice*x.Quantity * (decimal)(x.Discount + x.ExtraDiscount));
            detailsInvoiceTable.AddCell(Cell($"Remise: {Math.Round(discounts,2)}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"SHP: {Math.Round(_order.OrderItems.Sum(c => c.PFS),2)}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TOTAL HT: {Math.Round(_order.OrderTotal - totalTax,2)}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell("", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TOTAL TVA : {totalTax}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TTC:{Math.Round(_order.OrderTotal,2)}", BaseColor.BLACK));
            doc.Add(detailsInvoiceTable);

            #endregion

            var totalTTC = NumberToWordsHelper.NumberToText(_order.OrderTotal, false);
            Paragraph totalTTCText=  new Paragraph($"ARRETE LA PRESENTE A LA SOMME DE {totalTTC}",new Font(Font.NORMAL, 12, Font.ITALIC, BaseColor.BLACK));
            doc.Add(totalTTCText);
            doc.Close();
            await SavePdfFile(stream, string.Concat(strFilePath ,fileName));
            return string.Concat(strFilePath ,   fileName);
        }
        private static PdfPCell Cell(string property, BaseColor color, int border = 1, int paddingTop = 5, int paddingBottom = 5, BaseColor colorBorder = null, int size = 8, int font = Font.NORMAL)
        {
            return new PdfPCell(SetPhrase(property, color, size,(Font.FontFamily) font))
            {
                HorizontalAlignment = Element.ALIGN_LEFT, 
                Padding = 2,
                Border  = Rectangle.BOX,
                BorderColor = colorBorder == null ? BaseColor.BLACK : BaseColor.WHITE,
                PaddingTop = paddingTop,
                PaddingBottom = paddingBottom,

            };
        }
        private static Paragraph SetPhrase(string property, BaseColor color, int size = 8, Font.FontFamily font = Font.NORMAL)
        {
            return new Paragraph(property,new Font(font, size, Font.ITALIC, color));
        }
        private async Task  SavePdfFile(MemoryStream stream, string filename)
        {
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;
            await File.WriteAllBytesAsync(filename, byteInfo);
        }
    }
}