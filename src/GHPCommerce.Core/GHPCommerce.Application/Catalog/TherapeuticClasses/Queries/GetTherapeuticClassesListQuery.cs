using GHPCommerce.Application.Catalog.TherapeuticClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.TherapeuticClasses.Queries
{
    public class GetTherapeuticClassesListQuery :CommonListQuery, ICommand<PagingResult<TherapeuticClassDto>>
    {
        public GetTherapeuticClassesListQuery(string tem, string sort, int page, int pageSize) : base(tem, sort, page, pageSize)
        {
        }
       
    }
}
