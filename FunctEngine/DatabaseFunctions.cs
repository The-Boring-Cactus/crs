using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctEngine
{
    public class DatabaseFunctions
    {
        private readonly DatabaseManager databaseManager;

        public DatabaseFunctions(DatabaseManager databaseManager)
        {
            this.databaseManager = databaseManager;
        }

        public object ConnectPostgres(object[] args)
        {
            if (args.Length < 2) return false;
            string connectionName = args[0]?.ToString() ?? "";
            string connectionString = args[1]?.ToString() ?? "";
            return databaseManager.ConnectPostgres(connectionName, connectionString);
        }

        public object ConnectSqlServer(object[] args)
        {
            if (args.Length < 2) return false;
            string connectionName = args[0]?.ToString() ?? "";
            string connectionString = args[1]?.ToString() ?? "";
            return databaseManager.ConnectSqlServer(connectionName, connectionString);
        }

        public object DisconnectDB(object[] args)
        {
            if (args.Length == 0) return false;
            string connectionName = args[0]?.ToString() ?? "";
            return databaseManager.Disconnect(connectionName);
        }

        public object ExecuteQuery(object[] args)
        {
            if (args.Length < 2) return new List<object>();
            string connectionName = args[0]?.ToString() ?? "";
            string query = args[1]?.ToString() ?? "";

            var parameters = new object[args.Length - 2];
            Array.Copy(args, 2, parameters, 0, parameters.Length);

            return databaseManager.ExecuteQuery(connectionName, query, parameters);
        }

        public object ExecuteNonQuery(object[] args)
        {
            if (args.Length < 2) return 0;
            string connectionName = args[0]?.ToString() ?? "";
            string query = args[1]?.ToString() ?? "";

            var parameters = new object[args.Length - 2];
            Array.Copy(args, 2, parameters, 0, parameters.Length);

            return databaseManager.ExecuteNonQuery(connectionName, query, parameters);
        }

        public object ExecuteScalar(object[] args)
        {
            if (args.Length < 2) return null;
            string connectionName = args[0]?.ToString() ?? "";
            string query = args[1]?.ToString() ?? "";

            var parameters = new object[args.Length - 2];
            Array.Copy(args, 2, parameters, 0, parameters.Length);

            return databaseManager.ExecuteScalar(connectionName, query, parameters);
        }

        public object GetRowValue(object[] args)
        {
            if (args.Length < 2) return null;
            if (args[0] is Dictionary<string, object> row)
            {
                string columnName = args[1]?.ToString() ?? "";
                return row.ContainsKey(columnName) ? row[columnName] : null;
            }
            return null;
        }

        public object GetRowKeys(object[] args)
        {
            if (args.Length == 0) return new List<object>();
            if (args[0] is Dictionary<string, object> row)
            {
                return new List<object>(row.Keys);
            }
            return new List<object>();
        }

        public object BeginTransaction(object[] args)
        {
            if (args.Length == 0) return false;
            string connectionName = args[0]?.ToString() ?? "";
            return databaseManager.BeginTransaction(connectionName);
        }

        public object CommitTransaction(object[] args)
        {
            if (args.Length == 0) return false;
            string connectionName = args[0]?.ToString() ?? "";
            return databaseManager.CommitTransaction(connectionName);
        }

        public object RollbackTransaction(object[] args)
        {
            if (args.Length == 0) return false;
            string connectionName = args[0]?.ToString() ?? "";
            return databaseManager.RollbackTransaction(connectionName);
        }
    }
}
