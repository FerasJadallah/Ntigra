using Ntigra.DTOs;

namespace Ntigra.Services;

public interface IPatientService
{
    Task<PatientResponse?> CreatePatientAsync(CreatePatientRequest request);
    Task<PatientResponse?> GetPatientByIdAsync(int id);
    Task<List<PatientResponse>> GetAllPatientsAsync();
    Task<PatientResponse?> UpdatePatientAsync(int id, UpdatePatientRequest request);
    Task<bool> DeletePatientAsync(int id);
}
