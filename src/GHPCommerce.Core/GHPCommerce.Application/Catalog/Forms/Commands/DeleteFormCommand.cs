using System;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Forms.Commands
{
    public class DeleteFormCommand :ICommand
    {
        public Guid Id { get; set; }
    }
}
