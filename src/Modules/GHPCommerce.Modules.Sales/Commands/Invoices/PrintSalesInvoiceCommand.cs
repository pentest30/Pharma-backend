using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Application.Tiers.Organizations.Queries;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Identity;
using GHPCommerce.Domain.Repositories;
using GHPCommerce.Domain.Services;
using GHPCommerce.Infra.OS.Print;
using GHPCommerce.Modules.Sales.Entities.Billing;
using GHPCommerce.Modules.Sales.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Commands.Invoices
{
    public class PrintSalesInvoiceCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }

    public class PrintSalesInvoiceCommandHandler : ICommandHandler<PrintSalesInvoiceCommand, ValidationResult>
    {
        private readonly IRepository<Invoice, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly PrinterOptions _printerOptions;
        private static readonly SemaphoreSlim Slim = new SemaphoreSlim(1, 1);
        public PrintSalesInvoiceCommandHandler(
            IRepository<Invoice, Guid> repository,
            ICurrentOrganization currentOrganization,
            ICurrentUser currentUser,
            ICommandBus commandBus, PrinterOptions printerOptions)
        {
            _repository = repository;
            _currentOrganization = currentOrganization;
            _commandBus = commandBus;
            _currentUser = currentUser;
            _printerOptions = printerOptions;
        }

        public async Task<ValidationResult> Handle(PrintSalesInvoiceCommand request, CancellationToken cancellationToken)
        {

            try
            {
                await Slim.WaitAsync(cancellationToken);
                var invoice = await _repository.Table.Include(c => c.InvoiceItems)
                    .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
                var pdfHelper = new InvoiceOrderToPdfHelper(invoice, _commandBus);
                var path = await pdfHelper.GenerateInvoicePdfFileAsync();
                Print(path);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //throw;
            }
            finally
            {
                Slim.Release();
            }

            return default;
        }

        private  void Print(string filePath)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                var printerIp = _printerOptions.Printers.LastOrDefault(); 
                PrintHelper printHelper = new PrintHelper(fileBytes, printerIp);
                printHelper.PrintData();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Error");
            }

        }
    }
}