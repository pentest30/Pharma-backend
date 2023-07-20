using System;

namespace GHPCommerce.Domain.Domain.Common
{
    public interface IEntitySequenceNumber
    {
        public int SequenceNumber { get; set; }
        public Guid OrganizationId { get; set; }
    }
}