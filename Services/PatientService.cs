using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ntigra.Data;
using Ntigra.DTOs;
using Ntigra.Models;

namespace Ntigra.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        AppDbContext context,
        IAuditService auditService,
        ILogger<PatientService> logger)
    {
        _context = context;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<PatientResponse?> CreatePatientAsync(CreatePatientRequest request)
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

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create patient (inherits from User)
            var patient = new Patient
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                Role = "Patient",
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                Phone = request.Phone,
                Address = request.Address,
                CreatedAt = DateTime.UtcNow
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            _auditService.AddAuditLog(
                action: "CREATE",
                entityType: "Patient",
                entityId: patient.Id,
                details: JsonSerializer.Serialize(new
                {
                    patient.Id,
                    patient.Email,
                    patient.FirstName,
                    patient.LastName
                }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Patient created: {Email}, Id: {Id}", patient.Email, patient.Id);

            return new PatientResponse
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Phone = patient.Phone,
                Email = patient.Email,
                Address = patient.Address,
                CreatedAt = patient.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating patient");
            return null;
        }
    }

    public async Task<PatientResponse?> GetPatientByIdAsync(int id)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return null;

            _auditService.AddAuditLog(
                action: "READ",
                entityType: "Patient",
                entityId: patient.Id);
            await _context.SaveChangesAsync();

            return new PatientResponse
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Phone = patient.Phone,
                Email = patient.Email,
                Address = patient.Address,
                CreatedAt = patient.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by id: {Id}", id);
            return null;
        }
    }

    public async Task<List<PatientResponse>> GetAllPatientsAsync()
    {
        try
        {
            var patients = await _context.Patients
                .Select(p => new PatientResponse
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    Phone = p.Phone,
                    Email = p.Email,
                    Address = p.Address,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            _auditService.AddAuditLog(
                action: "READ_ALL",
                entityType: "Patient",
                entityId: 0,
                details: JsonSerializer.Serialize(new { Count = patients.Count }));
            await _context.SaveChangesAsync();

            return patients;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all patients");
            return new List<PatientResponse>();
        }
    }

    public async Task<PatientResponse?> UpdatePatientAsync(int id, CreatePatientRequest request)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return null;

            var before = new
            {
                patient.FirstName,
                patient.LastName,
                patient.DateOfBirth,
                patient.Phone,
                patient.Email,
                patient.Address
            };

            patient.FirstName = request.FirstName;
            patient.LastName = request.LastName;
            patient.DateOfBirth = request.DateOfBirth;
            patient.Phone = request.Phone;
            patient.Email = request.Email;
            patient.Address = request.Address;

            _auditService.AddAuditLog(
                action: "UPDATE",
                entityType: "Patient",
                entityId: patient.Id,
                details: JsonSerializer.Serialize(new
                {
                    Before = before,
                    After = new
                    {
                        patient.FirstName,
                        patient.LastName,
                        patient.DateOfBirth,
                        patient.Phone,
                        patient.Email,
                        patient.Address
                    }
                }));
            await _context.SaveChangesAsync();

            _logger.LogInformation("Patient updated: {PatientId}", id);

            return new PatientResponse
            {
                Id = patient.Id,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Phone = patient.Phone,
                Email = patient.Email,
                Address = patient.Address,
                CreatedAt = patient.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating patient: {Id}", id);
            return null;
        }
    }

    public async Task<bool> DeletePatientAsync(int id)
    {
        try
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return false;

            _auditService.AddAuditLog(
                action: "DELETE",
                entityType: "Patient",
                entityId: patient.Id,
                details: JsonSerializer.Serialize(new
                {
                    patient.Id,
                    patient.Email,
                    patient.FirstName,
                    patient.LastName
                }));
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Patient deleted: {PatientId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient: {Id}", id);
            return false;
        }
    }

}
