using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Connectors
{
    public abstract class BaseConnector : IConnector, IDisposable
    {
        protected readonly string _connectionString;
        protected DbConnection _connection;

        public abstract string Name { get; }
        public abstract string Type { get; }

        protected BaseConnector(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected abstract DbConnection CreateConnection();

        protected DbConnection GetOpenConnection()
        {
            if (_connection == null)
                _connection = CreateConnection();

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            return _connection;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var conn = CreateConnection();
                await conn.OpenAsync();
                await conn.CloseAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters = null)
        {
            var conn = GetOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null)
            {
                foreach (var (key, value) in parameters)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = key.StartsWith("@") ? key : $"@{key}";
                    param.Value = value ?? DBNull.Value;
                    cmd.Parameters.Add(param);
                }
            }

            var results = new List<Dictionary<string, object>>();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                results.Add(row);
            }

            return results;
        }

        public async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters = null)
        {
            var conn = GetOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null)
            {
                foreach (var (key, value) in parameters)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = key.StartsWith("@") ? key : $"@{key}";
                    param.Value = value ?? DBNull.Value;
                    cmd.Parameters.Add(param);
                }
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object> ExecuteScalarAsync(string sql, Dictionary<string, object> parameters = null)
        {
            var conn = GetOpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            if (parameters != null)
            {
                foreach (var (key, value) in parameters)
                {
                    var param = cmd.CreateParameter();
                    param.ParameterName = key.StartsWith("@") ? key : $"@{key}";
                    param.Value = value ?? DBNull.Value;
                    cmd.Parameters.Add(param);
                }
            }

            var result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? null : result;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _connection = null;
        }
    }
}
