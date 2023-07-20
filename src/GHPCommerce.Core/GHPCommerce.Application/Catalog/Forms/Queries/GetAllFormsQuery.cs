using System.Collections.Generic;
using GHPCommerce.Application.Catalog.Forms.DTOs;
using GHPCommerce.Domain.Domain.Commands;

namespace GHPCommerce.Application.Catalog.Forms.Queries
{
    public class GetAllFormsQuery : ICommand<IEnumerable<FormDto>>
    {
    }
}
