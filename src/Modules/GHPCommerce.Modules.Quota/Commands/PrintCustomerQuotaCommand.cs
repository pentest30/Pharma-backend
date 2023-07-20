using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Modules.Quota.Queries;
using OfficeOpenXml;

namespace GHPCommerce.Modules.Quota.Commands
{
    public class PrintCustomerQuotaCommand : ICommand<string>
    {
        
    }
    public class  PrintCustomerQuotaCommandHandler : ICommandHandler<PrintCustomerQuotaCommand, string>
    {
        private readonly ICommandBus _commandBus;

        public PrintCustomerQuotaCommandHandler(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
        public  async Task<string> Handle(PrintCustomerQuotaCommand request, CancellationToken cancellationToken)
        {
            var result = await _commandBus.SendAsync(new GetAllCustomersQuotaQuery(), cancellationToken);
            var strFilePath = Path.GetTempPath();
            string excelName = $"quota-{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage package = new ExcelPackage( new FileInfo (string.Concat(strFilePath, excelName))))
            {
                ExcelWorksheet sl = package.Workbook.Worksheets.Add("quota");
                var  j = 0;
                var e = 3;
                foreach (var item in result.GroupBy(x=>x.CustomerCode))
                {
                    sl.Cells[e + j, 1].Value = "" ;
                    foreach (var quotaDto in item)
                    {
                        sl.Cells[e+ 1 + j, 1].Value = quotaDto.CustomerCode;
                        sl.Cells[e+ 1 + j, 2].Value = quotaDto.CustomerName;
                        sl.Cells[e+ 1 + j, 3].Value = quotaDto.ProductCode;
                        sl.Cells[e+ 1 + j, 4].Value = quotaDto.ProductName;
                        sl.Cells[e+ 1 + j, 5].Value = quotaDto.QuotaDate;
                        sl.Cells[e+ 1 + j, 6].Value = quotaDto.InitQuantity;
                        j++;
                    }
                    e++;
                }
               
                for (var i = 1; i <= sl.Dimension.End.Column; i++) { 	sl.Column(i).AutoFit(); }
                await package.SaveAsync(cancellationToken);
                return string.Concat(strFilePath, excelName);
            }

        }
    }
}