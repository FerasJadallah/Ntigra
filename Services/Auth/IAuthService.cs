using Ntigra.DTOs;

namespace Ntigra.Services;

public interface IAuthService
{
    Task<RegisterResponse?> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
