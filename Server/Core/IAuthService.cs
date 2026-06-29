namespace Server.Core;


public interface IAuthService
{
    Task<string> RegisterUserAsync(RegisterRequest request);
    Task<string> AuthenticateAsync(LoginRequest request);
    Task<User> GetUserAsync(string userId);
    Task<bool> ValidateTokenAsync(string token);
    string GetUserIdFromToken(string token);
    Task<bool> UpdateUserProfileAsync(string userId, string displayName);
    Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword);
}