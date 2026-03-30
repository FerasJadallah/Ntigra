using Microsoft.EntityFrameworkCore;
using Ntigra.Data;

namespace Ntigra.Services;

public abstract class EmployeeServiceBase : ServiceBase
{
    protected EmployeeServiceBase(AppDbContext context) : base(context) { }

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
