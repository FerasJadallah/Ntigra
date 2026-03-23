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
    private readonly ICacheService _cache; 
    private readonly ILogger<PatientService> _logger;

    public PatientService(
        AppDbContext context,
        IAuditService auditService,
        ICacheService cache,
        ILogger<PatientService> logger)
    {
        _context = context;
        _auditService = auditService;
        _cache = cache;
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

            var existingUsername = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUsername != null)
            {
                _logger.LogWarning("Username already exists: {Username}", request.Username);
                return null;
            }

            // Hash password
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create patient (inherits from User)
            var patient = new Patient
            {
                Email = request.Email,
                Username = request.Username,
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

            // Invalidate cache for all patients since list changed
            await _cache.RemoveAsync("patients:all");

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
            // Try cache first
            var cacheKey = $"patient:{id}";
            var cached = await _cache.GetAsync<PatientResponse>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogInformation("✅ CACHE HIT for patient {PatientId}", id);
                return cached;
            }

            _logger.LogInformation("❌ CACHE MISS for patient {PatientId}, fetching from database", id);
            
            // Cache miss - get from database
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return null;

            var response = new PatientResponse
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

            // Store in cache for 5 minutes
            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));
            
            _auditService.AddAuditLog("READ", "Patient", patient.Id);
            await _context.SaveChangesAsync();

            return response;
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
            // Try cache first
            var cacheKey = "patients:all";
            var cached = await _cache.GetAsync<List<PatientResponse>>(cacheKey);
            
            if (cached != null)
            {
                _logger.LogInformation("✅ CACHE HIT for all patients");
                return cached;
            }

            _logger.LogInformation("❌ CACHE MISS for all patients, fetching from database");
            
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

            // Store in cache for 5 minutes
            await _cache.SetAsync(cacheKey, patients, TimeSpan.FromMinutes(5));

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

    public async Task<PatientResponse?> UpdatePatientAsync(int id, UpdatePatientRequest request)
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

            // Update only the fields that should be updatable
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

            // 👇 INVALIDATE CACHE - remove old data
            await _cache.RemoveAsync($"patient:{id}");
            await _cache.RemoveAsync("patients:all");

            _logger.LogInformation("Patient updated and cache invalidated: {PatientId}", id);

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

            // 👇 INVALIDATE CACHE
            await _cache.RemoveAsync($"patient:{id}");
            await _cache.RemoveAsync("patients:all");

            _logger.LogInformation("Patient deleted and cache invalidated: {PatientId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting patient: {Id}", id);
            return false;
        }
    }

    public async Task<PatientResponse?> GetPatientByUserIdAsync(int userId)
    {
        try
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == userId);
            
            if (patient == null)
                return null;

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
            _logger.LogError(ex, "Error getting patient by user id: {UserId}", userId);
            return null;
        }
    }
}
