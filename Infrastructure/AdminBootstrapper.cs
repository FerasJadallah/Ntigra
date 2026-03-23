using Microsoft.EntityFrameworkCore;
using Ntigra.Data;
using Ntigra.Models;

namespace Ntigra.Infrastructure;

public static class AdminBootstrapper
{
    public static async Task<bool> TryHandleAsync(string[] args, WebApplication app)
    {
        var parsed = SeedAdminOptions.TryParse(args, out var options, out var errorMessage);
        if (!parsed)
        {
            return false;
        }

        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("AdminBootstrapper");

        if (options is null)
        {
            logger.LogError("{Error}", errorMessage ?? BuildUsage());
            Environment.ExitCode = 1;
            return true;
        }

        try
        {
            await using var scope = app.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await context.Database.MigrateAsync();

            var existingEmail = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == options.Email);

            if (existingEmail is not null)
            {
                logger.LogError("Cannot create admin. Email already exists: {Email}", options.Email);
                Environment.ExitCode = 1;
                return true;
            }

            var existingUsername = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == options.Username);

            if (existingUsername is not null)
            {
                logger.LogError("Cannot create admin. Username already exists: {Username}", options.Username);
                Environment.ExitCode = 1;
                return true;
            }

            var admin = new Admin
            {
                Email = options.Email,
                Username = options.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(options.Password),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow,
                FirstName = options.FirstName,
                LastName = options.LastName,
                Department = options.Department,
                IsSuperAdmin = options.IsSuperAdmin
            };

            context.Admins.Add(admin);
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Admin created successfully. Id: {Id}, Email: {Email}, Username: {Username}, SuperAdmin: {IsSuperAdmin}",
                admin.Id,
                admin.Email,
                admin.Username,
                admin.IsSuperAdmin);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed admin account.");
            Environment.ExitCode = 1;
            return true;
        }
    }

    private static string BuildUsage()
    {
        return "Usage: dotnet run --project Ntigra -- seed-admin " +
               "--email <email> --username <username> --password <password> " +
               "--first-name <first name> --last-name <last name> " +
               "[--department <department>] [--super-admin <true|false>]";
    }

    private sealed class SeedAdminOptions
    {
        public required string Email { get; init; }
        public required string Username { get; init; }
        public required string Password { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public string? Department { get; init; }
        public bool IsSuperAdmin { get; init; } = true;

        public static bool TryParse(string[] args, out SeedAdminOptions? options, out string? errorMessage)
        {
            options = null;
            errorMessage = null;

            var seedIndex = Array.FindIndex(
                args,
                arg => string.Equals(arg, "seed-admin", StringComparison.OrdinalIgnoreCase));

            if (seedIndex < 0)
            {
                return false;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = seedIndex + 1; i < args.Length; i++)
            {
                var current = args[i];
                if (!current.StartsWith("--", StringComparison.Ordinal))
                {
                    errorMessage = $"Unexpected argument '{current}'. {BuildUsage()}";
                    return true;
                }

                var withoutPrefix = current[2..];
                var separatorIndex = withoutPrefix.IndexOf('=');

                string key;
                string value;

                if (separatorIndex >= 0)
                {
                    key = withoutPrefix[..separatorIndex];
                    value = withoutPrefix[(separatorIndex + 1)..];
                }
                else
                {
                    key = withoutPrefix;

                    if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
                    {
                        errorMessage = $"Missing value for '--{key}'. {BuildUsage()}";
                        return true;
                    }

                    value = args[++i];
                }

                values[key] = value;
            }

            if (!TryGetRequired(values, "email", out var email) ||
                !TryGetRequired(values, "username", out var username) ||
                !TryGetRequired(values, "password", out var password) ||
                !TryGetRequired(values, "first-name", out var firstName) ||
                !TryGetRequired(values, "last-name", out var lastName))
            {
                errorMessage = $"Missing one or more required arguments. {BuildUsage()}";
                return true;
            }

            var isSuperAdmin = true;
            if (values.TryGetValue("super-admin", out var superAdminValue) &&
                !bool.TryParse(superAdminValue, out isSuperAdmin))
            {
                errorMessage = $"Invalid value for '--super-admin': '{superAdminValue}'. Expected true or false.";
                return true;
            }

            options = new SeedAdminOptions
            {
                Email = email,
                Username = username,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                Department = values.GetValueOrDefault("department"),
                IsSuperAdmin = isSuperAdmin
            };

            return true;
        }

        private static bool TryGetRequired(
            IReadOnlyDictionary<string, string> values,
            string key,
            out string value)
        {
            if (values.TryGetValue(key, out value!) && !string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            value = string.Empty;
            return false;
        }
    }
}
