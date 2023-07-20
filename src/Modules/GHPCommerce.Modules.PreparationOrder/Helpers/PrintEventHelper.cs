
using System;
using System.Windows.Documents;
using GHPCommerce.Modules.PreparationOrder.Commands;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace GHPCommerce.Modules.PreparationOrder.Helpers
{
    
    public class PrintEventHelper: PdfPageEventHelper
    {
        private readonly Entities.PreparationOrder _preparationOrder;
        private readonly IPDFCommande _request;
        // This is the contentbyte object of the writer
        // This is the contentbyte object of the writer
        PdfContentByte cb;
        // we will put the final number of pages in a template
        PdfTemplate template;
        // this is the BaseFont we are going to use for the header / footer
        BaseFont bf = null;
        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;
        #region Properties
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }

        private string _HeaderLeft;
        public string HeaderLeft
        {
            get { return _HeaderLeft; }
            set { _HeaderLeft = value; }
        }
        private string _HeaderRight;
        public string HeaderRight
        {
            get { return _HeaderRight; }
            set { _HeaderRight = value; }
        }
        private Font _HeaderFont;
        public Font HeaderFont
        {
            get { return _HeaderFont; }
            set { _HeaderFont = value; }
        }
        private Font _FooterFont;
        public Font FooterFont
        {
            get { return _FooterFont; }
            set { _FooterFont = value; }
        }
        #endregion
        // we override the onOpenDocument method

        public PrintEventHelper(Entities.PreparationOrder preparationOrder, IPDFCommande request)
        {
            _preparationOrder = preparationOrder;
            _request = request;
        }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                PrintTime = DateTime.Now;
                bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                
                cb = writer.DirectContent;
                template = cb.CreateTemplate(80, 50);
            }
            catch (DocumentException de)
            {
            }
            catch (System.IO.IOException ioe)
            {
            }
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
           float[] widths = new float[] { 130f, 200f, 100f, 100 };
            PdfPTable tableHeader = new PdfPTable(4);
            //tableHeader.HeaderRows = 1;
            tableHeader.SetTotalWidth(widths);
            tableHeader.HorizontalAlignment = Element.ALIGN_LEFT;

            tableHeader.AddCell(new PdfPCell(new Phrase("" , new Font(Font.NORMAL, 8, 1, BaseColor.BLACK)))
            {
            
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 2,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            });
            tableHeader.AddCell(new PdfPCell(new Phrase("CMD  - "  + _preparationOrder.OrderIdentifier, 
                new Font(Font.NORMAL, 16, Font.BOLD, BaseColor.BLACK)))
            {

                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 2,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            });
            if(writer.PageNumber==1)
            tableHeader.AddCell(new PdfPCell(new Phrase("Colis: " +
                (_preparationOrder.ZoneGroupName!="B1"? "": _preparationOrder.TotalPackage.ToString())
                , new Font(Font.NORMAL, 8, 1, BaseColor.BLACK)))
            {

                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment=Element.ALIGN_CENTER,               
                Padding = 2,
                BackgroundColor = BaseColor.WHITE,
                BorderColor=BaseColor.DARK_GRAY,
                FixedHeight =20f
            });
            else
                tableHeader.AddCell(new PdfPCell(new Phrase("", new Font(Font.NORMAL, 8, 1, BaseColor.BLACK)))
                {
                     
                    BackgroundColor = BaseColor.WHITE,
                    Border = Rectangle.NO_BORDER
                });
            tableHeader.AddCell(new PdfPCell(new Phrase("", new Font(Font.NORMAL, 8, 1, BaseColor.BLACK)))
            {
            
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 2,
                PaddingBottom = 15,
                BackgroundColor = BaseColor.WHITE,
                Border = Rectangle.NO_BORDER
            });

            tableHeader.SpacingAfter = 20f;
            tableHeader.HeaderRows = 1;
            //document.SetMargins(25, 25, 25, 25);
            tableHeader.WriteSelectedRows(0, -1, document.LeftMargin, document.PageSize.Height -20f  , cb);

        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            int pageN = writer.PageNumber;
            String text = "Page " + pageN + " sur ";
            float len = bf.GetWidthPoint(text, 8);
            
            Rectangle pageSize = document.PageSize;
            cb.SetRGBColorFill(0,0,0);
            
            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            
            cb.SetTextMatrix(pageSize.GetLeft(10), pageSize.GetTop(20));
            cb.ShowText(text);
            cb.EndText();
            cb.AddTemplate(template, pageSize.GetLeft(10) + len , pageSize.GetTop(20));

            cb.BeginText();
            cb.SetFontAndSize(bf, 8);
            cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT,
                          "Page " + (pageN + _request.FirstPageNumber - 1) + " sur " + _request.TotalPageCount+" / Commande"
           
                , 
                pageSize.GetRight(20), 
                pageSize.GetTop(20), 0);
            cb.EndText();
        }
        

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);
            template.BeginText();
            template.SetFontAndSize(bf, 8);
            template.SetTextMatrix(0, 0);
            template.ShowText("" + (writer.PageNumber )+" / Groupe de zones");
            template.EndText();
        }
    }
}