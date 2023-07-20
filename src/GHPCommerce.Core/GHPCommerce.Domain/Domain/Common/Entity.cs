using System;
using System.ComponentModel.DataAnnotations;

namespace GHPCommerce.Domain.Domain.Common
{
    public abstract class Entity<TKey> : IHasKey<TKey>, ITrackable
    {
        public TKey Id { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        public DateTimeOffset CreatedDateTime { get; set; }
        public Guid CreatedByUserId { get; set; }
        public Guid? UpdatedByUserId { get; set; }

        public DateTimeOffset? UpdatedDateTime { get; set; }
    }
}
