
using GHPCommerce.Application.Catalog.PharmacologicalClasses.DTOs;
using GHPCommerce.Domain.Domain.Commands;
using GHPCommerce.Domain.Domain.Queries;

namespace GHPCommerce.Application.Catalog.PharmacologicalClasses.Queries
{
    public class GetPharmacologicalClassesListQuery :CommonListQuery, ICommand<PagingResult<PharmacologicalClassDto>>
    {
        public GetPharmacologicalClassesListQuery(string term, string sort, int page, int pageSize)
            : base(term, sort, page, pageSize)
        {

        }

    }
}
