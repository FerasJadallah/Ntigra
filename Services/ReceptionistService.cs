using Microsoft.EntityFrameworkCore;
using Ntigra.Data;
using Ntigra.DTOs;
using Ntigra.Models;
using System.Text.Json;

namespace Ntigra.Services;

public class ReceptionistService : IReceptionistService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<ReceptionistService> _logger;

    public ReceptionistService(
        AppDbContext context,
        IAuditService auditService,
        ILogger<ReceptionistService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ReceptionistResponse?> CreateReceptionistAsync(CreateReceptionistRequest request)
    {
        try
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            // Check if email exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            
            if (existingUser != null)
            {
                _logger.LogWarning("Email already exists: {Email}", request.Email);
                return null;
            }

            // Check if employee ID exists
            var existingEmployee = await _context.Users
                .OfType<Receptionist>()
                .FirstOrDefaultAsync(r => r.EmployeeId == request.EmployeeId);
            
            if (existingEmployee != null)
            {
                _logger.LogWarning("Employee ID already exists: {EmployeeId}", request.EmployeeId);
                return null;
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create receptionist
            var receptionist = new Receptionist
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "Receptionist",
                EmployeeId = request.EmployeeId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Department = request.Department,
                HireDate = request.HireDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Receptionists.Add(receptionist);
            await _context.SaveChangesAsync();

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
                    receptionist.LastName
                }));
            await _context.SaveChangesAsync();
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
            var receptionist = await _context.Receptionists.FindAsync(id);
            if (receptionist == null)
                return null;

            _auditService.AddAuditLog(
                action: "READ",
                entityType: "Receptionist",
                entityId: receptionist.Id);
            await _context.SaveChangesAsync();

            return new ReceptionistResponse
            {
                Id = receptionist.Id,
                Email = receptionist.Email,
                EmployeeId = receptionist.EmployeeId,
                FirstName = receptionist.FirstName,
                LastName = receptionist.LastName,
                Department = receptionist.Department,
                HireDate = receptionist.HireDate,
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
            var receptionists = await _context.Receptionists
                .Select(r => new ReceptionistResponse
                {
                    Id = r.Id,
                    Email = r.Email,
                    EmployeeId = r.EmployeeId,
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Department = r.Department,
                    HireDate = r.HireDate,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            _auditService.AddAuditLog(
                action: "READ_ALL",
                entityType: "Receptionist",
                entityId: 0,
                details: JsonSerializer.Serialize(new { Count = receptionists.Count }));
            await _context.SaveChangesAsync();

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
            var receptionist = await _context.Receptionists.FindAsync(id);
            if (receptionist == null)
                return null;

            var before = new
            {
                receptionist.EmployeeId,
                receptionist.FirstName,
                receptionist.LastName,
                receptionist.Department,
                receptionist.HireDate
            };

            receptionist.EmployeeId = request.EmployeeId;
            receptionist.FirstName = request.FirstName;
            receptionist.LastName = request.LastName;
            receptionist.Department = request.Department;
            receptionist.HireDate = request.HireDate;

            _auditService.AddAuditLog(
                action: "UPDATE",
                entityType: "Receptionist",
                entityId: receptionist.Id,
                details: JsonSerializer.Serialize(new
                {
                    Before = before,
                    After = new
                    {
                        request.EmployeeId,
                        request.FirstName,
                        request.LastName,
                        request.Department,
                        request.HireDate
                    }
                }));
            await _context.SaveChangesAsync();

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
            var receptionist = await _context.Receptionists.FindAsync(id);
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
                    receptionist.LastName
                }));
            _context.Receptionists.Remove(receptionist);
            await _context.SaveChangesAsync();

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
