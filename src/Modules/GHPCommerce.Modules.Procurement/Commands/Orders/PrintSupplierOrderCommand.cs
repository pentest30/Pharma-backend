using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Infra.OS.Print;
using GHPCommerce.Modules.Procurement.Entities;
using GHPCommerce.Modules.Procurement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Procurement.Commands.Orders
{
    public class PrintSupplierOrderCommand : ICommand<ValidationResult>
    { 
        public List<Guid> OrderIds { get; set; }
    }
     public  class  PrintSupplierOrderCommandHandler : ICommandHandler<PrintSupplierOrderCommand, ValidationResult>
     {
         private readonly IRepository<SupplierOrder, Guid> _repository;
         private readonly ICommandBus _commandBus;
         private readonly PrinterOptions _printerOptions;

         public PrintSupplierOrderCommandHandler(IRepository<SupplierOrder, Guid> repository, ICommandBus commandBus, PrinterOptions printerOptions)
         {
             _repository = repository;
             _commandBus = commandBus;
             _printerOptions = printerOptions;
         }
         public  async Task<ValidationResult> Handle(PrintSupplierOrderCommand request, CancellationToken cancellationToken)
         {
             foreach (var id in request.OrderIds)
             {
                 var order = await _repository.Table
                     .Include(x=>x.OrderItems)
                     .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                 var pdfHelper = new SupplierOrderToPdfHelper(order, _commandBus);
                 var path = await pdfHelper.GenerateOrderPdfFileAsync();
                 Print(path);
                 Thread.Sleep(500);
             }

             return default!;
         }

         private  async void Print(string filePath)
         {
             try
             {
                 byte[] fileBytes = File.ReadAllBytes(filePath);
                 var printerIp = _printerOptions.Printers[_printerOptions.DefaultPrinter - 1]; 
                 PrintHelper printHelper = new PrintHelper(fileBytes, printerIp);
                 await printHelper.PrintData();
             }
             catch (Exception ex)
             {
                 Console.WriteLine(ex.Message, "Error");
             }

         }
     }
}