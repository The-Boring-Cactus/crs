using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using Dapper;

namespace Server.Core;

public class AuthService : IAuthService
{
    private readonly ReportsCache _cache;
    
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
        object dbId = conn is MySqlConnector.MySqlConnection ? id.ToString() : id;
        
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
        if (passwordHash != user.PasswordHash)
            return null;
        
        return GenerateJwtToken(user.UserId);
    }
    
    public async Task<User> GetUserAsync(string userId)
    {
        using var conn = DatabasePersistence.CreateConnection();
        if (conn == null) return null;
        await conn.OpenAsync();
        
        object dbId = conn is MySqlConnector.MySqlConnection ? userId : Guid.Parse(userId);
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
            
            object dbId = conn is MySqlConnector.MySqlConnection ? userId : Guid.Parse(userId);
            var existing = await conn.QueryFirstOrDefaultAsync<string>("SELECT Id FROM Users WHERE Id = @Id AND IsActive = 1", new { Id = dbId });
            return existing != null;
        }
        catch
        {
            return false;
        }
    }
    
    public string GetUserIdFromToken(string token)
    {
        // Implementación simplificada - en producción usar JWT real
        var parts = token.Split('.');
        return parts.Length > 0 ? parts[0] : null;
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
        using var sha256 = SHA256.Create();
        var combined = Encoding.UTF8.GetBytes(password + salt);
        var hash = sha256.ComputeHash(combined);
        return Convert.ToBase64String(hash);
    }
    
    private string GenerateJwtToken(string userId)
    {
        return $"{userId}.{DateTime.UtcNow.Ticks}";
    }
    
    private class UserRecord
    {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public bool IsActive { get; set; }
    }
}