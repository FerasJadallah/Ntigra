using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ntigra.Data;
using Ntigra.DTOs;
using Ntigra.Services;

namespace Ntigra.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _context;  // 👈 Added

    public AuthController(IAuthService authService, AppDbContext context)  // 👈 Added context
    {
        _authService = authService;
        _context = context;  // 👈 Initialize
    }

[HttpPost("register")]
public async Task<IActionResult> Register(RegisterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var result = await _authService.RegisterAsync(request);

    if (result == null)
        return Conflict(new { message = "Email already registered or database error" });

    return Ok(result);
}

[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var result = await _authService.LoginAsync(request);

    if (result == null)
        return Unauthorized(new { message = "Invalid email or password" });

    return Ok(result);
}

    [HttpGet("test-roles")]
    public async Task<IActionResult> TestRoles()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.Role,
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }
}
