using System;
using System.IO;
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
using GHPCommerce.Modules.Sales.Entities.CreditNotes;
using GHPCommerce.Modules.Sales.Helpers;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;

namespace GHPCommerce.Modules.Sales.Commands.CreditNotes
{
    public class PrintCreditNoteCommand : ICommand<ValidationResult>
    {
        public Guid Id { get; set; }
    }

    public class PrintCreditNoteCommandHandler : ICommandHandler<PrintCreditNoteCommand, ValidationResult>
    {
        private readonly IRepository<CreditNote, Guid> _repository;
        private readonly ICurrentOrganization _currentOrganization;
        private readonly ICommandBus _commandBus;
        private readonly ICurrentUser _currentUser;
        private readonly PrinterOptions _printerOptions;
        public PrintCreditNoteCommandHandler(
            IRepository<CreditNote, Guid> repository,
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

        public async Task<ValidationResult> Handle(PrintCreditNoteCommand request, CancellationToken cancellationToken)
        {

            //var invoice = await _repository.Table.Include(c => c.CreditNoteItems).FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            //var pdfHelper = new CreditNoteToPdfHelper(invoice, _commandBus);
            //var path = await pdfHelper.GenerateCreditNotePdfFileAsync();
            ////  Print(path);
            //// Thread.Sleep(500);

            return default;
        }
        private async void Print(string filePath)
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