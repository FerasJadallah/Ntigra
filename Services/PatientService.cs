using Microsoft.EntityFrameworkCore;
using Ntigra.Data;
using Ntigra.DTOs;
using Ntigra.Models;

namespace Ntigra.Services;

public class PatientService : IPatientService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PatientService> _logger;

    public PatientService(AppDbContext context, ILogger<PatientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PatientResponse?> CreatePatientAsync(CreatePatientRequest request)
    {
        try
        {
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
            return await _context.Patients
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

            patient.FirstName = request.FirstName;
            patient.LastName = request.LastName;
            patient.DateOfBirth = request.DateOfBirth;
            patient.Phone = request.Phone;
            patient.Email = request.Email;
            patient.Address = request.Address;

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
