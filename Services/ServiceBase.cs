using Microsoft.EntityFrameworkCore;
using Ntigra.Data;

namespace Ntigra.Services;

public abstract class ServiceBase
{
    protected readonly AppDbContext Context;

    protected ServiceBase(AppDbContext context)
    {
        Context = context;
    }

    protected static string HashPassword(string password)
        => BCrypt.Net.BCrypt.HashPassword(password);

    protected async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        return await Context.Users.AnyAsync(u =>
            u.Email == email &&
            (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
    }

    protected async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
    {
        return await Context.Users.AnyAsync(u =>
            u.Username == username &&
            (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
    }
}
