using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ntigra.Data;
using Ntigra.DTOs;
using Ntigra.Models;

namespace Ntigra.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (existingUser != null)
            return Conflict(new { message = "Email already registered" });

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHash
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            message = "User registered successfully",
            userId = user.Id,
            email = user.Email
        });
    }
}
