using Microsoft.EntityFrameworkCore;
using Ntigra.Data;

namespace Ntigra.Services;

public abstract class EmployeeServiceBase
{
    protected readonly AppDbContext Context;

    protected EmployeeServiceBase(AppDbContext context)
    {
        Context = context;
    }

    protected static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

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

    protected async Task<bool> EmployeeIdExistsAsync(string employeeId, int? excludeEmployeeId = null)
    {
        return await Context.Employees.AnyAsync(e =>
            e.EmployeeId == employeeId &&
            (!excludeEmployeeId.HasValue || e.Id != excludeEmployeeId.Value));
    }

    protected async Task<string> GenerateEmployeeIdAsync(string prefix = "EMP")
    {
        var sequence = await Context.Employees.CountAsync() + 1;

        while (true)
        {
            var candidate = $"{prefix}-{sequence:D4}";

            if (!await EmployeeIdExistsAsync(candidate))
            {
                return candidate;
            }

            sequence++;
        }
    }
}
