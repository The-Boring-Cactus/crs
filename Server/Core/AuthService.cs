using System.Security.Cryptography;
using System.Text;

namespace Server.Core;

public class AuthService : IAuthService
{
    private readonly ReportsCache _cache;
    private readonly Dictionary<string, User> _users; // En producción usar DB
    
    public AuthService(ReportsCache cache)
    {
        _cache = cache;
        _users = new Dictionary<string, User>();
    }
    
    public async Task<string> RegisterUserAsync(RegisterRequest request)
    {
        if (_users.Values.Any(u => u.Username == request.Username))
            throw new InvalidOperationException("Username already exists");
        
        var salt = GenerateSalt();
        var passwordHash = HashPassword(request.Password, salt);
        
        var user = new User
        {
            UserId = Guid.NewGuid().ToString(),
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow
        };
        
        _users[user.UserId] = user;
        await Task.CompletedTask;
        
        return GenerateJwtToken(user);
    }
    
    public async Task<string> AuthenticateAsync(LoginRequest request)
    {
        var user = _users.Values.FirstOrDefault(u => u.Username == request.Username);
        if (user == null || !user.IsActive)
            return null;
        
        var passwordHash = HashPassword(request.Password, user.Salt);
        if (passwordHash != user.PasswordHash)
            return null;
        
        await Task.CompletedTask;
        return GenerateJwtToken(user);
    }
    
    public async Task<User> GetUserAsync(string userId)
    {
        await Task.CompletedTask;
        return _users.ContainsKey(userId) ? _users[userId] : null;
    }
    
    public async Task<bool> ValidateTokenAsync(string token)
    {
        await Task.CompletedTask;
        if (string.IsNullOrEmpty(token)) return false;
        
        try
        {
            var userId = GetUserIdFromToken(token);
            return _users.ContainsKey(userId);
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
    
    private string GenerateJwtToken(User user)
    {
        return $"{user.UserId}.{DateTime.UtcNow.Ticks}";
    }
}