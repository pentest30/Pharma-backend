using System;
using GHPCommerce.Application.Catalog.INNCodes.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.INNCodes.Commands
{
    public class CreateInnCodeLineCommand : ICommand
    {
        public Guid InnCodeId { get; set; }
        public InnCodeDosageDto InnCodeDosageDto { get; set; }
        public Guid Id { get; set; }
    }
}
