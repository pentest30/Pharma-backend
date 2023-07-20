using System;
using GHPCommerce.Application.Catalog.Forms.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Forms.Queries
{
    public class GetFormByIdQuery :ICommand<FormDto>
    {
        public Guid Id { get; set; }

    }
}
