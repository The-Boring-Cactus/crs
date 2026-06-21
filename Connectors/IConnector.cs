using System.Collections.Generic;
using System.Threading.Tasks;

namespace Connectors
{
    public interface IConnector
    {
        string Name { get; }
        string Type { get; }

        Task<bool> TestConnectionAsync();
        Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object> parameters = null);
        Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object> parameters = null);
        Task<object> ExecuteScalarAsync(string sql, Dictionary<string, object> parameters = null);
        void Dispose();
    }
}
