namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public class PostgreSQLGabbroException : Exception
    {
        public PostgreSQLGabbroException(string message) : base(message) { }
    }
}
