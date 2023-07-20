using GHPCommerce.CrossCuttingConcerns.ExtensionMethods;
using Microsoft.AspNetCore.Http;

namespace GHPCommerce.CrossCuttingConcerns.Exceptions
{
    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.Set(key, value.Serialize());
        }

        public static T Get<T>(this ISession session, string key)
        {
            session.TryGetValue(key, out  var  value);
            return value == null ? default : value.Deserialize<T>();
        }
    }
}
