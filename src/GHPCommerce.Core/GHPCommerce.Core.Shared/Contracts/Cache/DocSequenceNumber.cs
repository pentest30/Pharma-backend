using System;
using System.Collections.Generic;

namespace GHPCommerce.Core.Shared.Contracts.Cache
{
    public class DocSequenceNumbers
    {
        public DocSequenceNumbers()
        {
            SequenceNumbers = new List<DocSequenceNumber>();
        }
        public List<DocSequenceNumber> SequenceNumbers { get; set; }
    }
    public class DocSequenceNumber
    {
        public Guid DocId { get; set; }
        public int Year { get; set; }
    }
}