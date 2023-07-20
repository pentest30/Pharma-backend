namespace GHPCommerce.Persistence
{
    public class ConnectionStrings
    {
        public ConnectionStrings(string connectionString)
        {
            ConnectionString = connectionString;
        }
        public string ConnectionString { get; set; }
    }
}