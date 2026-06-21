using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Connectors
{
    public class MsSqlConnector : BaseConnector
    {
        public override string Name => "SQL Server";
        public override string Type => "mssql";

        public MsSqlConnector(string connectionString) : base(connectionString) { }

        public MsSqlConnector(string host, int port, string database, string username, string password)
            : base($"Server={host},{port};Database={database};User Id={username};Password={password};TrustServerCertificate=True;") { }

        protected override DbConnection CreateConnection()
            => new SqlConnection(_connectionString);
    }
}
