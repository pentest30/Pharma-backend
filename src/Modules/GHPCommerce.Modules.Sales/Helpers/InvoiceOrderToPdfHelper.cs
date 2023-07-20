using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GHPCommerce.Application.Tiers.Organizations.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Infra.OS.Print;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Queries;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;
using Rectangle = iTextSharp.text.Rectangle;

namespace GHPCommerce.Modules.Sales.Helpers
{
    public class InvoiceOrderToPdfHelper
    {
        private readonly Invoice _invoice;
        private readonly ICommandBus _commandBus;

        public InvoiceOrderToPdfHelper(Invoice invoice, ICommandBus commandBus)
        {
            _invoice = invoice;
            _commandBus = commandBus;
        }

        public async Task<string> GenerateInvoicePdfFileAsync()
        {
            Document doc = new Document(PageSize.A4, 5, 5, 40, 20);
            var strFilePath = Path.GetTempPath();
            var customer = await _commandBus.SendAsync(new GetOrganizationByIdQuery { Id = _invoice.CustomerId });
            var supplier = await _commandBus.SendAsync(new GetOrganizationByIdQuery { Id = _invoice.SupplierId });
            var order = await _commandBus.SendAsync(new GetOrderByIdQuery() { Id = _invoice.OrderId });

            var fileName = "Pdf_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".pdf";
            string[] headers = { "N°", "REF","DESIGNATION", "LOT","DDP","QTE","PU HT","PT HT","MRG","PPA HT", "PPA TTC", "SHP", "TVA" };
            float[] widths = new float[] { 5f, 12f, 30f, 10f, 15f, 10f, 10f, 12f, 8f, 12f, 12f, 7f, 7f };
            MemoryStream stream = new MemoryStream();
            PdfWriter.GetInstance(doc, stream).CloseStream = false;
            doc.SetMargins(5, 5, 5, 5); // 0.5 inch margins

            doc.Open();
            #region Entete facture Vente

            var supplierAddress = supplier?.Addresses?.FirstOrDefault(c => c.Main);
            Paragraph cp = new Paragraph("HYDRA PHARM SPA - Capital social: 1 616 025,000",
                new Font(Font.NORMAL, 12, Font.ITALIC, BaseColor.BLACK));
            Paragraph cp2 = new Paragraph("Distributeur de produits Pharmaceutiques et parapharmaceutiques",  new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            Paragraph  headOffice = new Paragraph($"SIEGE SOCIAL {supplierAddress?.Street} - {supplierAddress?.City}",  new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            Paragraph  supplierNisAiRc = new Paragraph($"NIS: {supplier?.NIS} - AI: {supplier?.AI} RC: {supplier?.RC}",  new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            Paragraph  supplierPhone = new Paragraph($"NIF {supplier?.NIF} - TEL: {supplier?.PhoneNumbers?.FirstOrDefault(x => !x.IsFax)}", new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            supplierPhone.SpacingAfter = 10;
            doc.Add(cp);
            doc.Add(cp2);
            doc.Add(headOffice);
            doc.Add(supplierNisAiRc);
            doc.Add(supplierPhone);
            #endregion

            #region Customer information
            Paragraph invoiceNumber =  new Paragraph($"Facture N° {_invoice.SequenceNumber}",new Font(Font.NORMAL, 14, Font.BOLDITALIC, BaseColor.BLACK));
            Paragraph salesOrderNumber = new Paragraph($"B_CMDE N° {order.OrderNumberSequence} DU {order.CreatedDateTime.Date.ToShortDateString()} - {_invoice.TotalPackage} COLIS -  {_invoice.TotalPackageThermolabile} COLIS FROID",new Font(Font.NORMAL, 12, Font.BOLDITALIC, BaseColor.BLACK));

            doc.Add(invoiceNumber);
            doc.Add(salesOrderNumber);
            doc.SetMargins(0, 0, 10, 10);
            float[] widthsCustomerTable = new float[] {10f, 50f, 40f};
            PdfPTable customerTable = new PdfPTable(3);
            customerTable.WidthPercentage = 100;

            var customerAddress = customer?.Addresses?.FirstOrDefault(c => c.Main);

            customerTable.SetWidths(widthsCustomerTable);
            customerTable.AddCell(Cell("Date Facture:", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE, 10, Font.BOLD));
            customerTable.AddCell(Cell($"{_invoice.CreatedDateTime.Date.ToShortDateString()}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10)
            );
            customerTable.AddCell(Cell($"Client: ", BaseColor.BLACK, 0,0, 0, BaseColor.WHITE, 10,Font.BOLD));
            
            customerTable.AddCell(Cell("RC Client: ", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10,Font.BOLD));
            customerTable.AddCell(Cell($"{customer?.RC}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            customerTable.AddCell(Cell($"{_invoice.CustomerName}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            
            customerTable.AddCell(Cell($"NIF Client: ", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10,Font.BOLD));
            customerTable.AddCell(Cell($"{customer?.NIF}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            customerTable.AddCell(Cell($"{customerAddress?.Street + customerAddress?.City}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE));
           
            customerTable.AddCell(Cell($"NIS Client:", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10,Font.BOLD));
            customerTable.AddCell(Cell($"{customer?.NIS}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            customerTable.AddCell(Cell("", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            
            customerTable.AddCell(Cell($"AI Client: ", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10,Font.BOLD));
            customerTable.AddCell(Cell($"{customer?.AI}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            customerTable.AddCell(Cell($"{customerAddress?.City}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            
            customerTable.AddCell(Cell($"Secteur: ", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10,Font.BOLD));
            customerTable.AddCell(Cell($"{_invoice.Sector}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            customerTable.AddCell(Cell($"TEL/Fax/Mail:{customer?.PhoneNumbers.FirstOrDefault(c => !c.IsFax)} / " +
                                       $"{customer?.PhoneNumbers?.FirstOrDefault(c => c.IsFax)}", BaseColor.BLACK, 0, 0, 0, BaseColor.WHITE,10));
            customerTable.SpacingBefore = 15;
            customerTable.SpacingAfter = 15;
            doc.Add(customerTable);

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
            foreach (var orderItem in _invoice.InvoiceItems)
            {
                
                pTable.AddCell(Cell("000".Substring(0,3 - orderItem.LineNum.ToString().Length )+ orderItem.LineNum.ToString(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.ProductCode.ToUpper(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.ProductName.ToUpper(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.InternalBatchNumber.ToUpper(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.ExpiryDate.ToShortDateString(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.Quantity.ToString(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.UnitPrice.ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.TotalExlTax.ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell((orderItem.DiscountRate * 100 ).ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.PpaHT.ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.PpaTTC.ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.PFS.ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.Tax.ToString(CultureInfo.InvariantCulture), BaseColor.BLACK));
            }
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell($"Total: {_invoice.TotalTTC}", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell($"{_invoice.TotalHT}", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));
            pTable.AddCell(Cell("", BaseColor.BLACK));


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

            var totalTax9 = _invoice.InvoiceItems.Where(c => c.Tax == 0.09).Sum(c => c.TotalTax); 
            var totalTax19 = _invoice.InvoiceItems.Where(c => c.Tax == 0.19).Sum(c => c.TotalTax);
            detailsInvoiceTable.AddCell(Cell($"PPA: {_invoice.InvoiceItems.Sum(c => c.PpaTTC)}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TVA 09%: {totalTax9}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TOTAL HT: {_invoice.TotalHT}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"MARGE: ", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell("", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"Remise: {_invoice.TotalDiscount}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"SHP: {_invoice.InvoiceItems.Sum(c => c.PFS)}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TVA 19%: {totalTax19}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TOTAL HT: {_invoice.TotalHT}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell("", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TOTAL TVA : {_invoice.TotalTax}", BaseColor.BLACK));
            detailsInvoiceTable.AddCell(Cell($"TTC:{_invoice.TotalTTC}", BaseColor.BLACK));
            doc.Add(detailsInvoiceTable);

            #endregion

            var totalTTC = NumberToWordsHelper.NumberToText(_invoice.TotalTTC, false);
            Paragraph totalTTCText=  new Paragraph($"ARRETE LA PRESENTE A LA SOMME DE {totalTTC}",new Font(Font.NORMAL, 12, Font.ITALIC, BaseColor.BLACK));
            doc.Add(totalTTCText);
            Paragraph reglement=  new Paragraph($"REGLEMENT:Par Cheque,{_invoice.NumberDueDays} jours( Au plus tard le  {_invoice.DueDate.ToShortDateString()} " +
                                                $"A defaut, le fournisseur s'autorise à reclamer de plein droit des astreintes journalières)",new Font(Font.NORMAL, 14, Font.ITALIC, BaseColor.BLACK));
            doc.Add(reglement);
            Paragraph generalCondition=  new Paragraph($"Extrait Conditions Générales de Vente",new Font(Font.NORMAL, 10, Font.ITALIC, BaseColor.BLACK));
            Paragraph generalConditionp1 =  new Paragraph("- Le Client est tenu de vérifier sa commande au moment de la livraison et d'accuser récéption sur la faccture.",new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.BLACK));
            Paragraph generalConditionp2 =  new Paragraph("- Toute réclamation sur les produits se fera dans un délai maximum de trois (03) jours à partir de la date de kleur réception",new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.BLACK));
            Paragraph generalConditionp3 =  new Paragraph("- Aucun retour, ni échange de produit ayant des propriétés psychtropes ou thermolabile ne sera accepté.",new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.BLACK));
            Paragraph generalConditionp4 =  new Paragraph("- Le client est tenu de collaborer pour la realisation de tout éventuel rappel de lot, il doit cesser la vente de lots concernés dés notification d'un rappel et respecter les délais de rappel qui lui sont commmuniqués.",new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.BLACK));
            Paragraph generalConditionp5 =  new Paragraph("- Le client devra transmettre toute information de pharmacovigilance, relative aux produits facturés, à son fournisseur, dans un délai n'excédant pas 24h..",new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.BLACK));
            Paragraph generalConditionp6 =  new Paragraph("- Tout produit ayant des propriétés psychotropes sera livré au client, expressément, selon les disposition du Décret executif n° 19-379 du 31 decembre 2019.",new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.BLACK));
            Paragraph generalConditionp7 =  new Paragraph("- Tout paiement effectué par le client servira à régler sa dette par ordre d'anteriorité.",new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.BLACK));
            generalCondition.SpacingBefore = 10;
            doc.Add(generalCondition);
            doc.Add(generalConditionp1);
            doc.Add(generalConditionp2);
            doc.Add(generalConditionp3);
            doc.Add(generalConditionp4);
            doc.Add(generalConditionp5);
            doc.Add(generalConditionp6);
            doc.Add(generalConditionp7);
           
            doc.Close();
            SavePdfFile(stream, string.Concat(strFilePath ,Path.DirectorySeparatorChar  + fileName));
            return string.Concat(strFilePath ,Path.DirectorySeparatorChar  + fileName);
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
        private static void SavePdfFile(MemoryStream stream, string filename)
        {
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;
            File.WriteAllBytes(filename, byteInfo);
        }
    }
}