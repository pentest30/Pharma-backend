
using Scriban;

namespace GHPCommerce.Modules.PreparationOrder.Helpers
{
    public class LabelZplHelper
    {
        public static string GetZplLabelTemplate(LabelConsolidationModel model)
        {


             var template = Template.Parse(@"
             ^XA
            ^CFd0,10,18
            ^PR12
            ^LRY
            ^MD30
            ^PW1000
            ^LL500
            ^PON
            ^CF0,20
            ^FO20,20^GB750,450,3^FS
            ^FO150,20^GB3,450,3^FS
            ^FO20,80^GB750,3,3^FS
            ^FO20,140^GB750,3,3^FS
            ^FO20,200^GB750,3,3^FS
            ^FO20,260^GB750,3,3^FS
            ^FO25,35^ADN,8,10^FD Client: ^FS
            ^FO180,35^ADN,36,8^FD{{model.customername}}^FS
            ^FO25,95^ADN,8,8^FD Secteur: ^FS
            ^FO180,95^ADN,36,8^FD{{model.sector}}^FS
            ^FO25,155^ADN,8,8^FD Colis: ^FS
            ^FO180,155^ADN,36,8^FDAmb {{model.totalpackage}} - Frigo {{model.totalpackagethermolabile}}^FS
            ^FO25,215^ADN,8,8^FD Ref N  ^FS
            ^FO180,215^ADN,36,8^FD{{model.orderidentifier}} - Zones {{model.zones}} ^FS
            ^FO25,400^ADN,8,8^FD^FS^BY5
            ^FO180,290^BC,100,Y,N,N,A^FD{{model.orderidentifier}}^^FS
            ^XZ"
            );

            var result = template.Render(new { model });
            return result;
        }
    }
    public class LabelConsolidationModel
    {
        public string Customername { get; set; }
        public string sector { get; set; }
        public string Barcode { get; set; }

        public string Orderidentifier { get; set; }
        public string Zones { get; set; }

        public int Totalpackage { get; set; }
        public int Totalpackagethermolabile { get; set; }
    }
}
