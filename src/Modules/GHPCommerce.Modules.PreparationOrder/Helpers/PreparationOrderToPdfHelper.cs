using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using iTextSharp.text;
using iTextSharp.text.pdf;
using GHPCommerce.Modules.PreparationOrder.Entities;
using GHPCommerce.Modules.PreparationOrder.Commands;

namespace GHPCommerce.Modules.PreparationOrder.Helpers
{
    public class PreparationOrderToPdfHelper
    {
        private readonly Entities.PreparationOrder _preparationOrder;
        private readonly ICommandBus _commandBus;
        private readonly IPDFCommande _request;

        public PreparationOrderToPdfHelper(Entities.PreparationOrder preparationOrder, ICommandBus commandBus, IPDFCommande request)
        {
            _preparationOrder = preparationOrder;
            _commandBus = commandBus;
            _request = request;
        }

        public async Task<byte[]> GeneratePreparationOrderToPdf()
        {
            PrintEventHelper e = new PrintEventHelper(_preparationOrder,_request);

            Document doc = new Document(PageSize.A4);

            doc.SetMargins(15, 15, 50, 0); // 0.5 inch margins
            var fileStream = new System.IO.MemoryStream();
            //using var fileStream = new ;// new FileStream(Path.GetTempPath() + _preparationOrder.SequenceNumber + ".pdf", FileMode.Create);
            PdfWriter writer = null;
            while (writer == null)
                try
                {
                     writer = PdfWriter.GetInstance(doc, fileStream); // sometime rise exception on first call

                    break;
                }
                catch
                {
                    System.Threading.Thread.Sleep(250); // wait for a while...
                }
            writer.PageEvent = e;
            doc.Open();
            doc.SetMargins(doc.LeftMargin, doc.RightMargin, doc.TopMargin + 10, doc.BottomMargin);
 

            PdfPTable zoneGroupTable = addZoneTable(_preparationOrder);
            zoneGroupTable.KeepTogether = false;
            zoneGroupTable.SplitLate = true;
            zoneGroupTable.SplitRows = true;
            zoneGroupTable.HorizontalAlignment = Element.ALIGN_LEFT;
            zoneGroupTable.WidthPercentage = 100;
            doc.Add(zoneGroupTable);
 
            var items = _preparationOrder.PreparationOrderItems
                .OrderBy(c => c.PickingZoneOrder)
                .ThenBy(c => c.PickingZoneName)
                .ThenBy(c => c.DefaultLocation)
                .ThenBy(c=>c.ProductName).GroupBy(c => c.PickingZoneId);
            int k = 1;
            int lineCountPerPage = 10;
            int initialLineCountPerPage = 10;
            foreach (var item in items)
            {

                if ( _request.ZonesOnTopPage.Contains(item.FirstOrDefault()?.PickingZoneName.ToUpper()))
                {
                    doc.NewPage();
                    lineCountPerPage = 160/40+initialLineCountPerPage; 

                }
                int skip = 0; 
                for (int i= 0;i<item.Count();
                i += (k < 2 && i == 0) ? 10 : 14
                    /*i+=(i+14<item.Count()?14: item.Count()-(i+1))*/)
                {
                    skip += ( i == 0) ? 0 : ((k < 2 && i == 10 ?10 :  14));
                    if (skip > item.Count()) break;
                     int take = (k < 2 && i == 0) ? 10 : 14;
                    PdfPTable zonePickingTable = addPickingZoneTable(_preparationOrder, item.Key,skip,take:take);
                    zonePickingTable.SetExtendLastRow(false, false);
                    zonePickingTable.KeepTogether = false;
                    zonePickingTable.SplitLate = true;
                    zonePickingTable.SplitRows = true;
                    zonePickingTable.SpacingAfter = 10;

                    zonePickingTable.HorizontalAlignment = Element.ALIGN_LEFT;
                    zonePickingTable.WidthPercentage = 100f;
                    doc.Add(zonePickingTable);
                    if ( i+ lineCountPerPage < item.Count()
                        || ( _request.ZonesOnTopPage.Contains(
                            item.FirstOrDefault()?.PickingZoneName.ToUpper())))
                    {
                        if ((k != items.Count() ||
                            i + lineCountPerPage < item.Count()
                            )) //not last page of the last zone in the group 
                            doc.NewPage();
                        lineCountPerPage = 160 / 40 + initialLineCountPerPage;

                    }
                }
                k++;
            }

 
            //writer.ResetPageCount();
            doc.Close();
            try
            {
                
                var bytes = fileStream.GetBuffer();
                await System.IO.File.WriteAllBytesAsync(Path.GetTempPath() + _preparationOrder.SequenceNumber + ".pdf", bytes);
                return bytes;
            }
            catch(Exception ex)
            {

            }
            return fileStream.GetBuffer(); ;
            //return Path.GetTempPath() + _preparationOrder.SequenceNumber + ".pdf";
        }
        private PdfPTable addZoneTable(Entities.PreparationOrder preparationOrder, PdfWriter writer = null)
        {
            PdfPTable tableZoneLayout = new PdfPTable(3);
            
            float[] widths = new float[] { 25f, 40f, 35f };
            tableZoneLayout.SetWidths(widths);
            var zonesString = preparationOrder.PreparationOrderItems.OrderBy(c => c.PickingZoneOrder).Select(c => c.PickingZoneName).Distinct().ToList();            
            var zonesGroupString = preparationOrder.PreparationOrderItems.Where(c => c.ZoneGroupId == preparationOrder.ZoneGroupId).OrderBy(c => c.PickingZoneOrder).Select(c => c.PickingZoneName).Distinct().ToList();


            //Zone Table 
            PdfPTable nested2 = new PdfPTable(1);
            PdfPCell cellWithRowspan = new PdfPCell(new Phrase(preparationOrder.ZoneGroupName?.ToUpper(), new Font(Font.FontFamily.HELVETICA, 30, Font.BOLD, BaseColor.BLACK)));
            cellWithRowspan.HorizontalAlignment = 1;
            cellWithRowspan.Padding = 5f;
            cellWithRowspan.Border = 0;
           
            nested2.AddCell(cellWithRowspan);
            var consolidator = _preparationOrder.ConsolidatedByName;
            if(string.IsNullOrEmpty(consolidator))
            nested2.AddCell(new PdfPCell(
                    new Phrase("Consolidateur", new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.DARK_GRAY)))
                {

                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_TOP,
                    Padding = 1,
                    Border = 0,

                });
            else
                nested2.AddCell(new PdfPCell(
        new Phrase($"Consolidateur\n{consolidator}", new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.DARK_GRAY)))
                {

                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_TOP,
                    Padding = 1,
                    Border = 0,

                });
            tableZoneLayout.AddCell(nested2);
            PdfPTable nested = new PdfPTable(2);
            
            AddCellToBody(nested,   preparationOrder.CustomerName.ToUpper(),fontSize:14);
            PdfPTable orderNumberTable = new PdfPTable(1);
            //orderNumberTable.AddCell(new PdfPCell(
            //        new Phrase("Code AX: " + preparationOrder.CodeAx, new Font(Font.NORMAL, 14, Font.BOLD, BaseColor.BLACK)))
            //{

            //    HorizontalAlignment = Element.ALIGN_LEFT,
            //    Padding = 2,
            //    BackgroundColor = BaseColor.WHITE,
            //}
            //);
            orderNumberTable.AddCell(new PdfPCell(
                    new Phrase("CMD N°: " + preparationOrder.OrderIdentifier+((preparationOrder.PreparationOrderStatus==PreparationOrderStatus.Controlled?
                    "\nContrôlé": (preparationOrder.PreparationOrderStatus == PreparationOrderStatus.Consolidated ?
                    "\nConsolidé" : (preparationOrder.PreparationOrderStatus == PreparationOrderStatus.ReadyToBeShipped ?
                    "\nA expédier" : ""
                    )
                    )
                    )), new Font(Font.NORMAL, 14, Font.BOLD, BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 2,
                Border=0,
                BorderColor=BaseColor.LIGHT_GRAY,FixedHeight=50
            }
            );
            
            
            nested.AddCell(orderNumberTable);
            AddCellToBody(nested, "Secteur: " + preparationOrder.SectorName,fixedHeight:30);
            AddCellToBody(nested, "Du:" + preparationOrder.CreatedDateTime.Day + "/" + preparationOrder.CreatedDateTime.ToString("dd/MM/yyyy HH:mm"), fixedHeight: 30);
            AddCellToBody(nested, "Zones: " + String.Join("/", zonesString.ToArray()), fixedHeight: 30);
            AddCellToBody(nested, $"Heure Imp ({_preparationOrder.PrintCount}) : " + 
                DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fixedHeight: 30);
            PdfPCell nesthousing = new PdfPCell(nested);
            tableZoneLayout.AddCell(nesthousing);

            var data = preparationOrder.BarCode;
            Image imageEAN = createCodeBar(data);
            imageEAN.ScaleToFit(100f, 50f);
            PdfPCell cellBarCoded = new PdfPCell(imageEAN, fit: true);
            cellBarCoded.Padding = 15;
            cellBarCoded.BorderColor = BaseColor.LIGHT_GRAY;
            tableZoneLayout.AddCell(cellBarCoded);
            AddCellToBody(tableZoneLayout, "Observation");
            AddCellToBody(tableZoneLayout, _request.ZonesStringByBL + ((preparationOrder.ToBeRespected) ? "  A respecter" : ""));
            AddCellToBody(tableZoneLayout, "Code Sect: " + preparationOrder.SectorCode);
            return tableZoneLayout;
        }

        private Image createCodeBar(string data)
        {

            var barcode = new NetBarcode.Barcode(data, NetBarcode.Type.Code128A, true);
            barcode.SaveImageFile(Path.GetTempPath() + "barCodeImage" + data, System.Drawing.Imaging.ImageFormat.Png);
            return Image.GetInstance(Path.GetTempPath() + "barCodeImage" + data);

        } 
        private PdfPTable addPickingZoneTable(Entities.PreparationOrder preparationOrder, Guid? pickingZoneId,int i, PdfWriter writer = null,int take=10)
        {

            var totalLineCount = preparationOrder.PreparationOrderItems.Where(c => pickingZoneId != null && c.PickingZoneId == pickingZoneId.Value).Count();
                var items = preparationOrder.PreparationOrderItems.Where(c => pickingZoneId != null && c.PickingZoneId == pickingZoneId.Value).Skip(i).Take(take).ToList();
            int index = preparationOrder.PreparationOrderItems.
                Where(c => pickingZoneId != null && c.PickingZoneId == pickingZoneId.Value).ToList()
                .FindIndex(it => it.Id == items[0].Id);
            var lineCountInThisPage = items.Count()+index;
            var pickingZoneOrder = (items.FirstOrDefault().PickingZoneOrder.ToString().Length == 2) ? items.FirstOrDefault()?.PickingZoneOrder.ToString() : 
                "00".Substring(0, 1 - items.FirstOrDefault().PickingZoneOrder.ToString().Length) + items.FirstOrDefault()?.PickingZoneOrder.ToString();
            var data = preparationOrder.BarCode + pickingZoneOrder;
            Image imageEAN = createCodeBar(data);
            PdfPCell cellBarCoded = new PdfPCell(imageEAN, fit: true);
            cellBarCoded.BorderColor = BaseColor.LIGHT_GRAY;
            cellBarCoded.BorderColorRight = BaseColor.WHITE;
            //cellBarCoded.Padding = 5;
            //
            //Add header
            int n = 7;

            if (_preparationOrder.ZoneGroupName.ToUpper() == "B1")
                n = 7;
            else
                n = 6;
            //n = 8;
            PdfPTable tableLayout = new PdfPTable(n);
            
            PdfPTable nestedtableLayout = new PdfPTable(4);
            float[] widthsCol = new float[] { 50f, 15f,30f,30f };
            nestedtableLayout.SetWidths(widthsCol);
            PdfPCell zoneRawSpan = new PdfPCell(new Phrase(items.FirstOrDefault()?.PickingZoneName.ToUpper(), new Font(Font.FontFamily.HELVETICA, 25, Font.BOLD, BaseColor.BLACK)));

            zoneRawSpan.HorizontalAlignment = 1;
            zoneRawSpan.VerticalAlignment = Element.ALIGN_CENTER;
            zoneRawSpan.Padding = 10f;
            zoneRawSpan.BorderColor = BaseColor.LIGHT_GRAY;
            zoneRawSpan.BorderColorLeft = BaseColor.WHITE;
            cellBarCoded.Padding = 15;
            zoneRawSpan.FixedHeight=cellBarCoded.FixedHeight = 120f;
            nestedtableLayout.AddCell(cellBarCoded);
            nestedtableLayout.AddCell(zoneRawSpan);
            var executors = String.Join('\n', _preparationOrder.PreparationOrderExecuters.Select(p => p.ExecutedByName));
            if (string.IsNullOrEmpty(executors))
                nestedtableLayout.AddCell(new PdfPCell(new Phrase("Préparateur", new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL, BaseColor.BLACK)))
                {
                    BorderColor = BaseColor.LIGHT_GRAY,
                    VerticalAlignment = Element.ALIGN_TOP,
                    PaddingTop = 5f,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            else
            {
                nestedtableLayout.AddCell(new PdfPCell(new Phrase("Préparateur\n\n"+ executors, new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL, BaseColor.BLACK)))
                {
                    BorderColor = BaseColor.LIGHT_GRAY,
                    VerticalAlignment = Element.ALIGN_TOP,
                    PaddingTop = 5f,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }
            var controllers = String.Join('\n', _preparationOrder.PreparationOrderVerifiers.Select(p => p.VerifiedByName));
            if (string.IsNullOrEmpty(controllers))
                nestedtableLayout.AddCell(new PdfPCell(new Phrase("Contrôleur", new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL, BaseColor.BLACK
                )))
            {
                BorderColor = BaseColor.LIGHT_GRAY,
                VerticalAlignment = Element.ALIGN_TOP,
                PaddingTop = 5f,
                HorizontalAlignment = Element.ALIGN_CENTER
            });
            else
                nestedtableLayout.AddCell(new PdfPCell(new Phrase("Contrôleur\n\n" + controllers, new Font(Font.FontFamily.HELVETICA, 8, Font.NORMAL, BaseColor.BLACK
    )))
                {
                    BorderColor = BaseColor.LIGHT_GRAY,
                    VerticalAlignment = Element.ALIGN_TOP,
                    PaddingTop = 5f,
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            if (_preparationOrder.ZoneGroupName.ToUpper() != "B1")
            {
                tableLayout.SetWidths(new float[] { 10f, 45f, 15f, 10f, 15f, 10f });
            }
            else
                tableLayout.SetWidths(new float[] { 10f, 45f, 15f, 10f, 10f, 15f, 10f});
            
            //AddCellToHeader(tableLayout, "Zone");
            AddCellToHeader(tableLayout, "ADRESSE");
            AddCellToHeader(tableLayout, "DESIGNATION");
            AddCellToHeader(tableLayout, "LOT");
            AddCellToHeader(tableLayout, "Q cmd");
            if(_preparationOrder.ZoneGroupName.ToUpper()=="B1")
            AddCellToHeader(tableLayout, "Colis");
            AddCellToHeader(tableLayout, "PPA");
            AddCellToHeader(tableLayout, "DDP");
            //AddCellToHeader(tableLayout, "RZ%");

            foreach (var item in items)
            {
                AddCellToBody(tableLayout, item.DefaultLocation?.ToUpper(),fontSize:14);
                AddCellToBody(tableLayout, (item.ExpiryDate.Value.AddMonths(-6)<=DateTime.Today?"* ":"")+
                    item.ProductName.ToUpper());
                AddCellToBody(tableLayout, item.InternalBatchNumber.ToUpper());
                tableLayout.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), new Font(Font.NORMAL, 10, Font.BOLD, BaseColor.BLACK)))
                {

                    HorizontalAlignment = Element.ALIGN_LEFT,
                    Padding = 5,
                    BackgroundColor = BaseColor.WHITE,
                    BorderColor=BaseColor.LIGHT_GRAY
                });
                if (_preparationOrder.ZoneGroupName.ToUpper() == "B1")
                    AddCellToBody(tableLayout, item.PackingQuantity.ToString());
                AddCellToBody(tableLayout, item.PpaHT.ToString());
                AddCellToBody(tableLayout, item.ExpiryDate.Value.ToString("MM/yy"));
                //AddCellToBody(tableLayout, (Math.Round(item.Discount, 2, MidpointRounding.ToEven)).ToString());
 
            } 
            PdfPCell cellWithRowspan = new PdfPCell(new Phrase("Nombre de lignes: " + 
                (lineCountInThisPage) +" / "+ totalLineCount, new Font(Font.NORMAL, 8, Font.NORMAL, BaseColor.BLACK)));
            if (_preparationOrder.ZoneGroupName.ToUpper() == "B1")
                cellWithRowspan.Colspan = 8;
            else
                cellWithRowspan.Colspan = 7;
            cellWithRowspan.Border = 0;
            cellWithRowspan.FixedHeight = 20f;
            cellWithRowspan.BorderColor = BaseColor.LIGHT_GRAY;
            tableLayout.AddCell(cellWithRowspan);
            
            PdfPCell nesthousing = new PdfPCell(tableLayout);
            nesthousing.Colspan = 4;
            nesthousing.BorderColor = BaseColor.LIGHT_GRAY;
            nestedtableLayout.AddCell(nesthousing);
            return nestedtableLayout;
        }

        // Method to add single cell to the header  
        private static void AddCellToHeader(PdfPTable tableLayout, string cellText)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.NORMAL, 8, 1, BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5,
                FixedHeight=20f,
                BorderColor = BaseColor.LIGHT_GRAY
            //BackgroundColor = new BaseColor(0, 51, 102)
        });
        }
        // Method to add single cell to the body  
        private static void AddCellToBody(PdfPTable tableLayout, string cellText,float fixedHeight=40f,int fontSize=10)
        {
            var font = new Font(Font.NORMAL, fontSize,1, BaseColor.BLACK); 
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, font))
            {

                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                BackgroundColor = BaseColor.WHITE,
                FixedHeight=fixedHeight,
                BorderColor = BaseColor.LIGHT_GRAY
            });
        }
    }
}