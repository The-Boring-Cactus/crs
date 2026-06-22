using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;

namespace Server.Core;

public class SetupController
{
    [ResourceMethod(RequestMethod.Get, "status")]
    public ValueTask<object> GetStatus()
    {
        var config = SetupConfig.Load();
        return ValueTask.FromResult<object>(new
        {
            configured = config.IsConfigured,
            databaseType = config.Database?.Type ?? "",
            configuredAt = config.ConfiguredAt
        });
    }

    [ResourceMethod(RequestMethod.Post, "test-connection")]
    public async ValueTask<object> TestConnection([FromBody] DatabaseConfig dbConfig)
    {
        try
        {
            var connectionString = dbConfig.GetConnectionString();
            DbConnection connection = dbConfig.Type?.ToLower() switch
            {
                "mssql" => new Microsoft.Data.SqlClient.SqlConnection(connectionString),
                "postgresql" => new Npgsql.NpgsqlConnection(connectionString),
                "mysql" => new MySqlConnector.MySqlConnection(connectionString),
                _ => throw new InvalidOperationException($"Unsupported database type: {dbConfig.Type}")
            };

            await using (connection)
            {
                await connection.OpenAsync();
                await connection.CloseAsync();
            }

            return new { success = true, message = "Connection successful" };
        }
        catch (Exception ex)
        {
            return new { success = false, message = ex.Message };
        }
    }

    [ResourceMethod(RequestMethod.Post, "complete")]
    public async ValueTask<object> CompleteSetup([FromBody] SetupRequest request)
    {
        try
        {
            // 1. Test DB connection
            var connectionString = request.Database.GetConnectionString();
            DbConnection connection = request.Database.Type?.ToLower() switch
            {
                "mssql" => new Microsoft.Data.SqlClient.SqlConnection(connectionString),
                "postgresql" => new Npgsql.NpgsqlConnection(connectionString),
                "mysql" => new MySqlConnector.MySqlConnection(connectionString),
                _ => throw new InvalidOperationException($"Unsupported database type: {request.Database.Type}")
            };

            await using (connection)
            {
                await connection.OpenAsync();

                // 2. Create tables
                var createTablesSql = request.Database.GetCreateTablesSQL();
                
                // Split and execute statements individually for MySQL compatibility
                var statements = SplitSqlStatements(createTablesSql, request.Database.Type);
                foreach (var statement in statements)
                {
                    if (string.IsNullOrWhiteSpace(statement)) continue;
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = statement;
                    await cmd.ExecuteNonQueryAsync();
                }

                // 3. Create admin user
                var salt = GenerateSalt();
                var passwordHash = HashPassword(request.AdminPassword, salt);
                var userId = Guid.NewGuid();

                var insertUserSql = request.Database.Type?.ToLower() switch
                {
                    "mssql" => @"INSERT INTO Users (Id, Username, FullName, Email, PasswordHash, Salt, Roles) 
                                 VALUES (@Id, @Username, @FullName, @Email, @PasswordHash, @Salt, @Roles)",
                    "postgres" => @"INSERT INTO Users (Id, Username, FullName, Email, PasswordHash, Salt, Roles) 
                                    VALUES (@Id, @Username, @FullName, @Email, @PasswordHash, @Salt, @Roles)",
                    "mysql" => @"INSERT INTO Users (Id, Username, FullName, Email, PasswordHash, Salt, Roles) 
                                 VALUES (@Id, @Username, @FullName, @Email, @PasswordHash, @Salt, @Roles)",
                    _ => throw new InvalidOperationException("Unsupported database type")
                };

                using var insertCmd = connection.CreateCommand();
                insertCmd.CommandText = insertUserSql;
                
                AddParameter(insertCmd, "@Id", request.Database.Type?.ToLower() == "mysql" ? userId.ToString() : userId);
                AddParameter(insertCmd, "@Username", request.AdminUser.Username);
                AddParameter(insertCmd, "@FullName", request.AdminUser.FullName);
                AddParameter(insertCmd, "@Email", request.AdminUser.Email);
                AddParameter(insertCmd, "@PasswordHash", passwordHash);
                AddParameter(insertCmd, "@Salt", salt);
                AddParameter(insertCmd, "@Roles", "admin");

                await insertCmd.ExecuteNonQueryAsync();
                await connection.CloseAsync();
            }

            // 4. Save config
            var config = new SetupConfig
            {
                Database = request.Database,
                AdminUser = request.AdminUser,
                Smtp = request.Smtp ?? new SmtpConfig(),
                IsConfigured = true,
                ConfiguredAt = DateTime.UtcNow
            };
            config.Save();

            return new { success = true, message = "Setup completed successfully" };
        }
        catch (Exception ex)
        {
            return new { success = false, message = ex.Message };
        }
    }

    private List<string> SplitSqlStatements(string sql, string dbType)
    {
        // For MSSQL, we can execute multi-statement blocks
        if (dbType?.ToLower() == "mssql")
        {
            return sql.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim())
                       .Where(s => !string.IsNullOrWhiteSpace(s))
                       .ToList();
        }
        
        // For PostgreSQL and MySQL, split on semicolons
        return sql.Split(';')
                   .Select(s => s.Trim())
                   .Where(s => !string.IsNullOrWhiteSpace(s))
                   .ToList();
    }

    private void AddParameter(DbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        cmd.Parameters.Add(param);
    }

    private string GenerateSalt()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private string HashPassword(string password, string salt)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(salt),
            100_000,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            32);
        return Convert.ToBase64String(hash);
    }
}

public class SetupRequest
{
    public DatabaseConfig Database { get; set; }
    public AdminUserConfig AdminUser { get; set; }
    public string AdminPassword { get; set; }
    public SmtpConfig Smtp { get; set; }
}
