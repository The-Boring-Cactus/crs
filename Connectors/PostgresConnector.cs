using System.Data.Common;
using Npgsql;

namespace Connectors
{
    public class PostgresConnector : BaseConnector
    {
        public override string Name => "PostgreSQL";
        public override string Type => "postgres";

        public PostgresConnector(string connectionString) : base(connectionString) { }

        public PostgresConnector(string host, int port, string database, string username, string password)
            : base($"Host={host};Port={port};Database={database};Username={username};Password={password};") { }

        protected override DbConnection CreateConnection()
            => new NpgsqlConnection(_connectionString);
    }
}
