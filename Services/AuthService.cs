using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ntigra.Data;
using Ntigra.DTOs;
using Ntigra.Models;

namespace Ntigra.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    // ============= REGISTER =============
    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Check if email exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - email already exists: {Email}", request.Email);
                return null;
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user (default role = "Patient")
            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "Patient",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User registered successfully: {Email}, Id: {UserId}", user.Email, user.Id);

            return new RegisterResponse
            {
                Message = "User registered successfully",
                UserId = user.Id,
                Email = user.Email,
                Role = user.Role
            };
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database error during registration for: {Email}", request.Email);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for: {Email}", request.Email);
            return null;
        }
    }

    // ============= LOGIN =============
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        try
        {
            // Find user by email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                return null;
            }

            // Verify password
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            
            if (!isValidPassword)
            {
                _logger.LogWarning("Login failed - invalid password for: {Email}", request.Email);
                return null;
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);
            
            _logger.LogInformation("User logged in successfully: {Email}, Role: {Role}", user.Email, user.Role);

            return new LoginResponse
            {
                Token = token,
                Email = user.Email,
                Role = user.Role,
                UserId = user.Id,
                Expiration = DateTime.UtcNow.AddMinutes(60)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error for: {Email}", request.Email);
            return null;
        }
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJwtTokenGeneration2026!");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"] ?? "60")),
            Issuer = _configuration["Jwt:Issuer"] ?? "NtigraAPI",
            Audience = _configuration["Jwt:Audience"] ?? "NtigraClient",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}