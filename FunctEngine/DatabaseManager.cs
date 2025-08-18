using Microsoft.Data.SqlClient;
using Npgsql;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class DatabaseManager : IDisposable
    {
        private readonly Dictionary<string, IDbConnection> connections = new Dictionary<string, IDbConnection>();
        private readonly Dictionary<string, IDbTransaction> transactions = new Dictionary<string, IDbTransaction>();

        public bool ConnectPostgres(string connectionName, string connectionString)
        {
            try
            {
                var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                connections[connectionName] = connection;
                Console.WriteLine($"Conectado a PostgreSQL: {connectionName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error conectando a PostgreSQL {connectionName}: {ex.Message}");
                return false;
            }
        }

        public bool ConnectSqlServer(string connectionName, string connectionString)
        {
            try
            {
                var connection = new SqlConnection(connectionString);
                connection.Open();
                connections[connectionName] = connection;
                Console.WriteLine($"Conectado a SQL Server: {connectionName}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error conectando a SQL Server {connectionName}: {ex.Message}");
                return false;
            }
        }

        public bool Disconnect(string connectionName)
        {
            if (connections.ContainsKey(connectionName))
            {
                try
                {
                    connections[connectionName].Close();
                    connections[connectionName].Dispose();
                    connections.Remove(connectionName);
                    Console.WriteLine($"Desconectado de: {connectionName}");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error desconectando {connectionName}: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        public List<object> ExecuteQuery(string connectionName, string query, params object[] parameters)
        {
            if (!connections.ContainsKey(connectionName))
            {
                Console.WriteLine($"Conexión no encontrada: {connectionName}");
                return new List<object>();
            }

            try
            {
                var connection = connections[connectionName];
                using (var command = connection.CreateCommand())
                {
                    // Si hay una transacción activa, usarla
                    if (transactions.ContainsKey(connectionName))
                    {
                        command.Transaction = transactions[connectionName];
                    }

                    command.CommandText = query;

                    // Agregar parámetros si los hay
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = $"@param{i + 1}";
                        parameter.Value = parameters[i] ?? DBNull.Value;
                        command.Parameters.Add(parameter);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        var results = new List<object>();
                        while (reader.Read())
                        {
                            var row = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                            }
                            results.Add(row);
                        }
                        return results;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ejecutando query en {connectionName}: {ex.Message}");
                return new List<object>();
            }
        }

        public int ExecuteNonQuery(string connectionName, string query, params object[] parameters)
        {
            if (!connections.ContainsKey(connectionName))
            {
                Console.WriteLine($"Conexión no encontrada: {connectionName}");
                return 0;
            }

            try
            {
                var connection = connections[connectionName];
                using (var command = connection.CreateCommand())
                {
                    // Si hay una transacción activa, usarla
                    if (transactions.ContainsKey(connectionName))
                    {
                        command.Transaction = transactions[connectionName];
                    }

                    command.CommandText = query;

                    // Agregar parámetros si los hay
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = $"@param{i + 1}";
                        parameter.Value = parameters[i] ?? DBNull.Value;
                        command.Parameters.Add(parameter);
                    }

                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ejecutando comando en {connectionName}: {ex.Message}");
                return 0;
            }
        }

        public object ExecuteScalar(string connectionName, string query, params object[] parameters)
        {
            if (!connections.ContainsKey(connectionName))
            {
                Console.WriteLine($"Conexión no encontrada: {connectionName}");
                return null;
            }

            try
            {
                var connection = connections[connectionName];
                using (var command = connection.CreateCommand())
                {
                    // Si hay una transacción activa, usarla
                    if (transactions.ContainsKey(connectionName))
                    {
                        command.Transaction = transactions[connectionName];
                    }

                    command.CommandText = query;

                    // Agregar parámetros si los hay
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = $"@param{i + 1}";
                        parameter.Value = parameters[i] ?? DBNull.Value;
                        command.Parameters.Add(parameter);
                    }

                    var result = command.ExecuteScalar();
                    return result == DBNull.Value ? null : result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ejecutando scalar en {connectionName}: {ex.Message}");
                return null;
            }
        }

        public bool BeginTransaction(string connectionName)
        {
            if (!connections.ContainsKey(connectionName))
            {
                Console.WriteLine($"Conexión no encontrada: {connectionName}");
                return false;
            }

            try
            {
                var connection = connections[connectionName];
                var transaction = connection.BeginTransaction();
                transactions[connectionName] = transaction;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error iniciando transacción en {connectionName}: {ex.Message}");
                return false;
            }
        }

        public bool CommitTransaction(string connectionName)
        {
            if (transactions.ContainsKey(connectionName))
            {
                try
                {
                    transactions[connectionName].Commit();
                    transactions[connectionName].Dispose();
                    transactions.Remove(connectionName);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error confirmando transacción en {connectionName}: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        public bool RollbackTransaction(string connectionName)
        {
            if (transactions.ContainsKey(connectionName))
            {
                try
                {
                    transactions[connectionName].Rollback();
                    transactions[connectionName].Dispose();
                    transactions.Remove(connectionName);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error revirtiendo transacción en {connectionName}: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        public void Dispose()
        {
            // Revertir todas las transacciones activas
            foreach (var transaction in transactions.Values)
            {
                try
                {
                    transaction?.Rollback();
                    transaction?.Dispose();
                }
                catch { }
            }
            transactions.Clear();

            // Cerrar todas las conexiones
            foreach (var connection in connections.Values)
            {
                try
                {
                    connection?.Close();
                    connection?.Dispose();
                }
                catch { }
            }
            connections.Clear();
        }
    }
}
