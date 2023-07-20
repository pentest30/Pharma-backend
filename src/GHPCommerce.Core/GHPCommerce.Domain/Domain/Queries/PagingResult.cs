using System.Collections.Generic;

namespace GHPCommerce.Domain.Domain.Queries
{
    public class PagingResult <T> where T : class
    {
        public IEnumerable<T> Data { get; set; }

        public int Total { get; set; }
    }
}
