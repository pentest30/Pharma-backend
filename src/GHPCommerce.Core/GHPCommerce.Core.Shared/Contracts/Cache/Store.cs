using System;
using System.Collections.Generic; 

namespace GHPCommerce.Core.Shared.Contracts.Cache
{
    [Serializable]
    public class Store
    {
        public Store(List<CachedKey> cachedKeys)
        {
            CachedKeys = cachedKeys;
        }

        public string Id { get; set; }
        public List<CachedKey> CachedKeys { get; set; }

    }

    [Serializable]
    public class CachedKey
    {
        public CachedKey(string typeName, string key, DateTime? expiresAt)
        {
            TypeName = typeName;
            Key = key;
            ExpiresAt = expiresAt;
        }
        public string TypeName { get; set; }
        public string Key { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
