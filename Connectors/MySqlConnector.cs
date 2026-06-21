using System.Data.Common;
using MySqlConnector;

namespace Connectors
{
    public class MySqlDbConnector : BaseConnector
    {
        public override string Name => "MySQL";
        public override string Type => "mysql";

        public MySqlDbConnector(string connectionString) : base(connectionString) { }

        public MySqlDbConnector(string host, int port, string database, string username, string password)
            : base($"Server={host};Port={port};Database={database};Uid={username};Pwd={password};") { }

        protected override DbConnection CreateConnection()
            => new MySqlConnection(_connectionString);
    }
}
