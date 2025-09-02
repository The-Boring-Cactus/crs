using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Webservices;

namespace Server.Core;

public class AuthController
{
    private readonly IAuthService _authService;
    
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }



    [ResourceMethod(RequestMethod.Post,"register")]
    public async ValueTask<AuthResponse> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var token = await _authService.RegisterUserAsync(request);
            return new AuthResponse { Token = token, Message = "User registered successfully" };
        }
        catch (InvalidOperationException ex)
        {
            throw new ProviderException(ResponseStatus.BadRequest, ex.Message);
        }
    }
    
    [ResourceMethod(RequestMethod.Post,"login")]
    public async ValueTask<AuthResponse> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.AuthenticateAsync(request);
        
        if (string.IsNullOrEmpty(token))
            throw new ProviderException(ResponseStatus.Unauthorized, "Invalid credentials");
        
        return new AuthResponse { Token = token, Message = "Login successful" };
    }
}
