using Microsoft.EntityFrameworkCore;
using Ntigra.Data;
using Ntigra.DTOs;
using Ntigra.Models;
using System.Text.Json;

namespace Ntigra.Services;

public class ReceptionistService : EmployeeServiceBase, IReceptionistService
{
    private readonly IAuditService _auditService;
    private readonly ILogger<ReceptionistService> _logger;

    public ReceptionistService(
        AppDbContext context,
        IAuditService auditService,
        ILogger<ReceptionistService> logger)
        : base(context)
    {
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ReceptionistResponse?> CreateReceptionistAsync(CreateReceptionistRequest request)
    {
        try
        {
            await using var transaction = await Context.Database.BeginTransactionAsync();

            // Check if email exists
            if (await EmailExistsAsync(request.Email))
            {
                _logger.LogWarning("Email already exists: {Email}", request.Email);
                return null;
            }

            if (await UsernameExistsAsync(request.Username))
            {
                _logger.LogWarning("Username already exists: {Username}", request.Username);
                return null;
            }

            // Hash password
            var passwordHash = HashPassword(request.Password);
            var employeeId = await GenerateEmployeeIdAsync();

            // Create receptionist
            var receptionist = new Receptionist
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = "Receptionist",
                EmployeeId = employeeId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                HireDate = request.HireDate,
                Salary = request.Salary,
                DeskNumber = request.DeskNumber,
                CreatedAt = DateTime.UtcNow
            };

            Context.Receptionists.Add(receptionist);
            await Context.SaveChangesAsync();

            _auditService.AddAuditLog(
                action: "CREATE",
                entityType: "Receptionist",
                entityId: receptionist.Id,
                details: JsonSerializer.Serialize(new
                {
                    receptionist.Id,
                    receptionist.Email,
                    receptionist.EmployeeId,
                    receptionist.FirstName,
                    receptionist.LastName,
                    receptionist.DeskNumber,
                    receptionist.Username
                }));
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Receptionist created: {Email}, Id: {Id}", receptionist.Email, receptionist.Id);

            return new ReceptionistResponse
            {
                Id = receptionist.Id,
                Email = receptionist.Email,
                EmployeeId = receptionist.EmployeeId,
                FirstName = receptionist.FirstName,
                LastName = receptionist.LastName,
                Department = receptionist.Department,
                HireDate = receptionist.HireDate,
                Salary = receptionist.Salary,
                DeskNumber = receptionist.DeskNumber,
                CreatedAt = receptionist.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating receptionist");
            return null;
        }
    }

    public async Task<ReceptionistResponse?> GetReceptionistByIdAsync(int id)
    {
        try
        {
            var receptionist = await Context.Receptionists.FindAsync(id);
            if (receptionist == null)
                return null;

            _auditService.AddAuditLog(
                action: "READ",
                entityType: "Receptionist",
                entityId: receptionist.Id);
            await Context.SaveChangesAsync();

            return new ReceptionistResponse
            {
                Id = receptionist.Id,
                Email = receptionist.Email,
                EmployeeId = receptionist.EmployeeId,
                FirstName = receptionist.FirstName,
                LastName = receptionist.LastName,
                Department = receptionist.Department,
                HireDate = receptionist.HireDate,
                Salary = receptionist.Salary,
                DeskNumber = receptionist.DeskNumber,
                CreatedAt = receptionist.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receptionist by id: {Id}", id);
            return null;
        }
    }

    public async Task<List<ReceptionistResponse>> GetAllReceptionistsAsync()
    {
        try
        {
            var receptionists = await Context.Receptionists
                .Select(r => new ReceptionistResponse
                {
                    Id = r.Id,
                    Email = r.Email,
                    EmployeeId = r.EmployeeId,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Department = r.Department,
                    HireDate = r.HireDate,
                    Salary = r.Salary,
                    DeskNumber = r.DeskNumber,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            _auditService.AddAuditLog(
                action: "READ_ALL",
                entityType: "Receptionist",
                entityId: 0,
                details: JsonSerializer.Serialize(new { Count = receptionists.Count }));
            await Context.SaveChangesAsync();

            return receptionists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all receptionists");
            return new List<ReceptionistResponse>();
        }
    }

    public async Task<ReceptionistResponse?> UpdateReceptionistAsync(int id, UpdateReceptionistRequest request)
    {
        try
        {
            var receptionist = await Context.Receptionists.FindAsync(id);
            if (receptionist == null)
                return null;

            var before = new
            {
                receptionist.EmployeeId,
                receptionist.FirstName,
                receptionist.LastName,
                receptionist.Department,
                receptionist.HireDate,
                receptionist.Salary,
                receptionist.DeskNumber
            };

            receptionist.FirstName = request.FirstName;
            receptionist.LastName = request.LastName;
            receptionist.Department = request.Department;
            receptionist.HireDate = request.HireDate;
            receptionist.Salary = request.Salary;
            receptionist.DeskNumber = request.DeskNumber;

            _auditService.AddAuditLog(
                action: "UPDATE",
                entityType: "Receptionist",
                entityId: receptionist.Id,
                details: JsonSerializer.Serialize(new
                {
                    Before = before,
                    After = new
                    {
                        receptionist.EmployeeId,
                        request.FirstName,
                        request.LastName,
                        request.Department,
                        request.HireDate,
                        request.Salary,
                        request.DeskNumber
                    }
                }));
            await Context.SaveChangesAsync();

            _logger.LogInformation("Receptionist updated: {Id}", id);

            return new ReceptionistResponse
            {
                Id = receptionist.Id,
                Email = receptionist.Email,
                EmployeeId = receptionist.EmployeeId,
                FirstName = receptionist.FirstName,
                LastName = receptionist.LastName,
                Department = receptionist.Department,
                HireDate = receptionist.HireDate,
                Salary = receptionist.Salary,
                DeskNumber = receptionist.DeskNumber,
                CreatedAt = receptionist.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating receptionist: {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeleteReceptionistAsync(int id)
    {
        try
        {
            var receptionist = await Context.Receptionists.FindAsync(id);
            if (receptionist == null)
                return false;

            _auditService.AddAuditLog(
                action: "DELETE",
                entityType: "Receptionist",
                entityId: receptionist.Id,
                details: JsonSerializer.Serialize(new
                {
                    receptionist.Id,
                    receptionist.Email,
                    receptionist.EmployeeId,
                    receptionist.FirstName,
                    receptionist.LastName,
                    receptionist.DeskNumber
                }));
            Context.Receptionists.Remove(receptionist);
            await Context.SaveChangesAsync();

            _logger.LogInformation("Receptionist deleted: {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting receptionist: {Id}", id);
            return false;
        }
    }

}
