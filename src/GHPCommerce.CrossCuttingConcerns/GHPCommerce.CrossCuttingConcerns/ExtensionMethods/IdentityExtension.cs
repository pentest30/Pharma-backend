using System;
using System.DirectoryServices.AccountManagement;
using System.Reflection;

namespace GHPCommerce.CrossCuttingConcerns.ExtensionMethods
{
    public static class IdentityExtension
    {
        public static string ExtensionGet(this UserPrincipal up, string key)
        {
            string value = null;
            MethodInfo mi = up.GetType()
                .GetMethod("ExtensionGet", BindingFlags.NonPublic | BindingFlags.Instance);

            Func<UserPrincipal, string, object[]> extensionGet = (k, v) =>
                (object[])mi.Invoke(k, new object[] { v });

            if (extensionGet(up, key).Length > 0)
            {
                value = (string)extensionGet(up, key)[0];
            }

            return value;
        }
    }
}
