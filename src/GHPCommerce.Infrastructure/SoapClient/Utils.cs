namespace SoapClient
{
    public static class Utils
    {
        public static string parseNs(string value)
        {
            string[] parts = value.Split(':');
            return parts.Length > 1 ? parts[1] : parts[0];
        }
    }
}
