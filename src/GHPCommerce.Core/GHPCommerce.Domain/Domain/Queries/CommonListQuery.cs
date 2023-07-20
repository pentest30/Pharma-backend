using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;

namespace GHPCommerce.Domain.Domain.Queries
{
    public class CommonListQuery
    {
        public CommonListQuery(string term, string sort, int page, int pageSize)
        {
            Term = term;
            SortDir = !string.IsNullOrEmpty(sort) ? sort.Split('_')[1] : string.Empty;
            SortProp = !string.IsNullOrEmpty(sort) && !sort.Contains("undefined") ? sort.Split('_')[0].UppercaseFirst() : string.Empty;
            Page = page;
            PageSize = pageSize;
        }

        protected CommonListQuery()
        {
           
        }

        public string Term { get; set; }
        public string SortDir { get; set; }
        public string SortProp { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
