using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using Newtonsoft.Json;

namespace Server.Core;

public class AuthService : IAuthService
{
    private readonly ReportsCache _cache;
    private static readonly string JwtSecretPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jwt_secret.key");
    private static readonly Lazy<byte[]> _jwtSecret = new(LoadOrCreateSecret);

    public AuthService(ReportsCache cache)
    {
        _cache = cache;
    }

    public async Task<string> RegisterUserAsync(RegisterRequest request)
    {
        using var conn = DatabasePersistence.CreateConnection();
        if (conn == null) throw new InvalidOperationException("System not configured");
        await conn.OpenAsync();

        var existing = await conn.QueryFirstOrDefaultAsync<string>("SELECT Id FROM Users WHERE Username = @Username", new { request.Username });
        if (existing != null)
            throw new InvalidOperationException("Username already exists");

        var salt = GenerateSalt();
        var passwordHash = HashPassword(request.Password, salt);
        var id = Guid.NewGuid();
        object dbId = (conn is MySqlConnector.MySqlConnection || DatabasePersistence.IsOracleConnection(conn)) ? id.ToString() : id;

        await conn.ExecuteAsync(@"INSERT INTO Users (Id, Username, FullName, Email, PasswordHash, Salt, Roles)
                                  VALUES (@Id, @Username, @FullName, @Email, @PasswordHash, @Salt, @Roles)",
            new { Id = dbId, Username = request.Username, FullName = request.Username, Email = request.Email, PasswordHash = passwordHash, Salt = salt, Roles = "user" });

        return GenerateJwtToken(id.ToString());
    }

    public async Task<string> AuthenticateAsync(LoginRequest request)
    {
        using var conn = DatabasePersistence.CreateConnection();
        if (conn == null) return null;
        await conn.OpenAsync();

        var user = await conn.QueryFirstOrDefaultAsync<UserRecord>("SELECT Id as UserId, Username, PasswordHash, Salt, IsActive FROM Users WHERE Username = @Username", new { request.Username });

        if (user == null || !user.IsActive)
            return null;

        var passwordHash = HashPassword(request.Password, user.Salt);
        if (passwordHash == user.PasswordHash)
            return GenerateJwtToken(user.UserId.ToString());

        // Legacy SHA256 fallback — migrate on first successful login with old hash
        var legacyHash = HashLegacySHA256(request.Password, user.Salt);
        if (legacyHash == user.PasswordHash)
        {
            var newHash = HashPassword(request.Password, user.Salt);
            object dbId = (conn is MySqlConnector.MySqlConnection || DatabasePersistence.IsOracleConnection(conn)) ? (object)user.UserId.ToString() : user.UserId;
            await conn.ExecuteAsync("UPDATE Users SET PasswordHash = @Hash WHERE Id = @Id", new { Hash = newHash, Id = dbId });
            return GenerateJwtToken(user.UserId.ToString());
        }

        return null;
    }

    public async Task<User> GetUserAsync(string userId)
    {
        using var conn = DatabasePersistence.CreateConnection();
        if (conn == null) return null;
        await conn.OpenAsync();

        object dbId = (conn is MySqlConnector.MySqlConnection || DatabasePersistence.IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
        return await conn.QueryFirstOrDefaultAsync<User>("SELECT Id as UserId, Username, Email, CreatedAt, IsActive, Roles FROM Users WHERE Id = @Id", new { Id = dbId });
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token)) return false;

        try
        {
            var userId = GetUserIdFromToken(token);
            if (string.IsNullOrEmpty(userId)) return false;

            using var conn = DatabasePersistence.CreateConnection();
            if (conn == null) return false;
            await conn.OpenAsync();

            object dbId = (conn is MySqlConnector.MySqlConnection || DatabasePersistence.IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
            // Query IsActive rather than comparing with a literal to be DB-agnostic
            var isActive = await conn.QueryFirstOrDefaultAsync<bool?>(
                "SELECT IsActive FROM Users WHERE Id = @Id", new { Id = dbId });
            return isActive == true;
        }
        catch
        {
            return false;
        }
    }

    public string GetUserIdFromToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;

            var expectedSig = CreateHmacSignature($"{parts[0]}.{parts[1]}");
            if (!CryptographicEquals(expectedSig, parts[2])) return null;

            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            var payload = JsonConvert.DeserializeObject<Dictionary<string, object>>(payloadJson);

            if (payload == null) return null;

            var exp = payload.ContainsKey("exp") ? Convert.ToInt64(payload["exp"]) : 0L;
            if (exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds()) return null;

            return payload.ContainsKey("sub") ? payload["sub"]?.ToString() : null;
        }
        catch
        {
            return null;
        }
    }

    private string GenerateSalt()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private string HashLegacySHA256(string password, string salt)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(password + salt));
        return Convert.ToBase64String(hash);
    }

    private string HashPassword(string password, string salt)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Encoding.UTF8.GetBytes(salt),
            100_000,
            HashAlgorithmName.SHA256,
            32);
        return Convert.ToBase64String(hash);
    }

    private string GenerateJwtToken(string userId)
    {
        var header = Base64UrlEncode(Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(new { alg = "HS256", typ = "JWT" })));

        var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(
            JsonConvert.SerializeObject(new
            {
                sub = userId,
                iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                exp = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds()
            })));

        var sig = CreateHmacSignature($"{header}.{payload}");
        return $"{header}.{payload}.{sig}";
    }

    private string CreateHmacSignature(string data)
    {
        using var hmac = new HMACSHA256(_jwtSecret.Value);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] data)
        => Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }

    private static bool CryptographicEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    private static byte[] LoadOrCreateSecret()
    {
        if (File.Exists(JwtSecretPath))
            return Convert.FromBase64String(File.ReadAllText(JwtSecretPath).Trim());

        var secret = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(secret);
        File.WriteAllText(JwtSecretPath, Convert.ToBase64String(secret));
        return secret;
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, string displayName)
    {
        using var conn = DatabasePersistence.CreateConnection();
        if (conn == null) return false;
        await conn.OpenAsync();

        object dbId = (conn is MySqlConnector.MySqlConnection || DatabasePersistence.IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
        var rows = await conn.ExecuteAsync(
            "UPDATE Users SET FullName = @FullName WHERE Id = @Id",
            new { FullName = displayName, Id = dbId });
        return rows > 0;
    }

    public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        using var conn = DatabasePersistence.CreateConnection();
        if (conn == null) return false;
        await conn.OpenAsync();

        object dbId = (conn is MySqlConnector.MySqlConnection || DatabasePersistence.IsOracleConnection(conn)) ? userId : Guid.Parse(userId);
        var user = await conn.QueryFirstOrDefaultAsync<UserRecord>(
            "SELECT Id as UserId, PasswordHash, Salt FROM Users WHERE Id = @Id",
            new { Id = dbId });
        if (user == null) return false;

        var currentHash = HashPassword(oldPassword, user.Salt);
        if (currentHash != user.PasswordHash)
        {
            var legacyHash = HashLegacySHA256(oldPassword, user.Salt);
            if (legacyHash != user.PasswordHash) return false;
        }

        var newHash = HashPassword(newPassword, user.Salt);
        await conn.ExecuteAsync("UPDATE Users SET PasswordHash = @Hash WHERE Id = @Id", new { Hash = newHash, Id = dbId });
        return true;
    }

    private class UserRecord
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public bool IsActive { get; set; }
    }
}
