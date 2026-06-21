using System;

namespace Connectors
{
    public static class ConnectorFactory
    {
        public static IConnector Create(string type, string connectionString)
        {
            return type?.ToLower() switch
            {
                "postgres" or "postgresql" => new PostgresConnector(connectionString),
                "mysql" => new MySqlDbConnector(connectionString),
                "mssql" or "sqlserver" => new MsSqlConnector(connectionString),
                _ => throw new ArgumentException($"Unsupported connector type: {type}")
            };
        }

        public static IConnector Create(string type, string host, int port, string database, string username, string password)
        {
            return type?.ToLower() switch
            {
                "postgres" or "postgresql" => new PostgresConnector(host, port, database, username, password),
                "mysql" => new MySqlDbConnector(host, port, database, username, password),
                "mssql" or "sqlserver" => new MsSqlConnector(host, port, database, username, password),
                _ => throw new ArgumentException($"Unsupported connector type: {type}")
            };
        }
    }
}
