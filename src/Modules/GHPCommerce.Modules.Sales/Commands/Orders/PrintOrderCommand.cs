using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Infra.OS.Print;
using GHPCommerce.Modules.Sales.Entities;
using GHPCommerce.Modules.Sales.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Commands.Orders
{
    public class PrintOrderCommand:ICommand<string>
    {
        public Guid OrderId { get; set; }
    }
    public class PrintOrderCommandHandler :ICommandHandler<PrintOrderCommand, string>
    {
        private readonly IRepository<Order, Guid> _repository;
        private readonly ICommandBus _commandBus;
        private readonly PrinterOptions _printerOptions;

        public PrintOrderCommandHandler(IRepository<Order, Guid> repository, ICommandBus commandBus, PrinterOptions printerOptions)
        {
            _repository = repository;
            _commandBus = commandBus;
            _printerOptions = printerOptions;
        }
        public async Task<string> Handle(PrintOrderCommand request, CancellationToken cancellationToken)
        {
            var order =await _repository.Table
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == request.OrderId, cancellationToken: cancellationToken);
            var pdfHelper = new OrderToPdfHelper(order, _commandBus);
            var path = await pdfHelper.GenerateInvoicePdfFileAsync();
            return path;
        }
        private async void Print(string filePath)
        {
            try
            {
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
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