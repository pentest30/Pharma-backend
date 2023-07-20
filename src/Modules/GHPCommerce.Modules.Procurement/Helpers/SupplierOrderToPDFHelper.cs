using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GHPCommerce.Application.Tiers.Organizations.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Procurement.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GHPCommerce.Modules.Procurement.Helpers
{
    public class SupplierOrderToPdfHelper
    {
        private readonly SupplierOrder _order;
        private readonly ICommandBus _commandBus;

        public SupplierOrderToPdfHelper(SupplierOrder order, ICommandBus commandBus)
        {
            _order = order;
            _commandBus = commandBus;
        }

        public async Task<string> GenerateOrderPdfFileAsync()
        {
            Document doc = new Document(PageSize.A4, 20, 20, 40, 20);
            var strFilePath = Path.GetTempPath();
            var customer = await _commandBus.SendAsync(new GetOrganizationByIdQuery { Id = _order.CustomerId });
            var supplier = await _commandBus.SendAsync(new GetOrganizationByIdQuery { Id = _order.SupplierId });

            var fileName = "Pdf_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".pdf";
            string[] headers = { "Désignation", "Quantité" };
            float[] columnWidths = { 800f, 140f };
            MemoryStream stream = new MemoryStream();
            PdfWriter.GetInstance(doc, stream).CloseStream = false;
            doc.Open();

            #region Entête

            PdfPTable tableHead = new PdfPTable(2)
            {
                WidthPercentage = 94
            };
            Paragraph cp = SetPhrase($"Capital social: 10 000 000,00", BaseColor.BLACK);
            Paragraph tel = SetPhrase($"Tél: {customer.PhoneNumbers.FirstOrDefault(x=>!x.IsFax)?.Number}", BaseColor.BLACK);
            Paragraph fax = SetPhrase($"Fax: {customer.PhoneNumbers.FirstOrDefault(x=>x.IsFax)?.Number}", BaseColor.BLACK);
            Image logo = Image.GetInstance("Images/logo.png");
            logo.ScaleToFit(50f,45f);
            PdfPCell leftCell = new PdfPCell
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 2,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            };
            leftCell.AddElement(cp);
            leftCell.AddElement(tel);
            leftCell.AddElement(fax);
          
            Paragraph orderHeader = SetPhrase($"Bon de Commande N°: {_order.RefDocument}", BaseColor.BLACK);
            Paragraph dateHeader = SetPhrase($"Date: {DateTime.Now:d}", BaseColor.BLACK);
            leftCell.AddElement(dateHeader);
            Paragraph empty = SetPhrase($"", BaseColor.BLACK);
            var rightCell = new PdfPCell
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 2,
                
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            };
            rightCell.AddElement(logo);
            rightCell.AddElement(empty);
            rightCell.AddElement(orderHeader);
            tableHead.AddCell(rightCell);
            tableHead.AddCell(leftCell);
            
            doc.Add(tableHead);
            doc.Add(SetPhrase("", BaseColor.WHITE));
            doc.Add(new Chunk(Environment.NewLine));

            #endregion

         
            doc.Add(SetPhrase("", BaseColor.BLACK));
            #region Informations client et fournisseur

            var address = customer.Addresses.FirstOrDefault(x => x.Main);
            var supplierAddress = supplier.Addresses.FirstOrDefault(x => x.Main);
            PdfPTable tableLayout = new PdfPTable(2)
            {
                WidthPercentage = 98
            };
            leftCell = new PdfPCell
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 10,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.BOX
            };
           // leftCell.Border = 0;
            leftCell.AddElement(SetPhrase($"Raison sociale: {customer.Name.ToUpper()}", BaseColor.BLACK));
            leftCell.AddElement(SetPhrase($"Adresse: {address?.Street }, {address?.City }, {address?.Country}", BaseColor.BLACK));
            leftCell.AddElement(SetPhrase($"Registre de commerce: {customer.RC}", BaseColor.BLACK));
            leftCell.AddElement(SetPhrase($"Article d'imposition: {customer.AI}", BaseColor.BLACK));
            leftCell.AddElement(SetPhrase($"N° Identification fiscale:  {customer.NIF}", BaseColor.BLACK));
            tableLayout.AddCell(leftCell);
            rightCell = new PdfPCell
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 10,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.BOX
            };

          //  rightCell.Border = 0;
            rightCell.AddElement(SetPhrase($"Raison sociale: {supplier.Name.ToUpper()}", BaseColor.BLACK));
            rightCell.AddElement(SetPhrase($"Adresse: {supplierAddress?.Street }, {supplierAddress?.City }, {supplierAddress?.Country}", BaseColor.BLACK));
            tableLayout.AddCell(rightCell);
            doc.Add(tableLayout);
          
            doc.Add(new Chunk(Environment.NewLine));

            #endregion

            #region lignes commande

            PdfPTable pTable = new PdfPTable(headers.Length);
            pTable.WidthPercentage = 98;
            pTable.SetWidths(columnWidths);
            foreach (string t in headers)
            {
                var cell = new PdfPCell(new Phrase(t, FontFactory.GetFont("Calibri", 10, BaseColor.BLACK)))
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    BackgroundColor = BaseColor.WHITE,
                    Padding = 5
                };
                pTable.AddCell(cell);
            }

            foreach (var orderItem in _order.OrderItems)
            {
                pTable.AddCell(Cell(orderItem.ProductName.ToUpper(), BaseColor.BLACK));
                pTable.AddCell(Cell(orderItem.Quantity.ToString("# ##0"), BaseColor.BLACK));

            }

            doc.Add(pTable);
            doc.Add(SetPhrase("", BaseColor.BLACK));
            doc.Add(Cell(@"Merci de ne pas facturer les produits à date de péremption inférieure à
            18 mois sans la validation au préalable de la part de HYDRA-PHARM", BaseColor.RED));
            #endregion

       
            doc.Close();
            SavePdfFile(stream, string.Concat(strFilePath ,Path.DirectorySeparatorChar  + fileName));
            return string.Concat(strFilePath ,Path.DirectorySeparatorChar  + fileName);
        }

        private static void SavePdfFile(MemoryStream stream, string filename)
        {
            byte[] byteInfo = stream.ToArray();
            stream.Write(byteInfo, 0, byteInfo.Length);
            stream.Position = 0;
            File.WriteAllBytes(filename, byteInfo);
        }
         static PdfPCell Cell(string property, BaseColor color)
        {
            return new PdfPCell(SetPhrase(property, color))
                { HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5, };
        }

         private static Paragraph SetPhrase(string property, BaseColor color)
         {
             return new Paragraph(property,
                 FontFactory.GetFont("Calibri", 10, color));
         }
    }
}