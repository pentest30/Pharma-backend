using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Modules.Sales.Commands.Invoices
{
    public class PrintBulkInvoicesCommand : ICommand<ValidationResult>
    {
        public List<Guid> Ids { get; set; }

    }

    public class PrintBulkInvoicesCommandHandler : ICommandHandler<PrintBulkInvoicesCommand, ValidationResult>
    {
        private readonly ICommandBus _commandBus;
        
        public PrintBulkInvoicesCommandHandler(ICommandBus commandBus)
        {
            _commandBus = commandBus;
        }
        public async Task<ValidationResult> Handle(PrintBulkInvoicesCommand request, CancellationToken cancellationToken)
        {
            for (int i = 0; i < request.Ids.Count; i++)
            {
                var id = request.Ids[i];
                await _commandBus.SendAsync(new PrintSalesInvoiceCommand { Id = id }, cancellationToken);

            }

            return default;
        }
    }
}