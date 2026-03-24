using Ntigra.DTOs;

namespace Ntigra.Services;

public interface IReceptionistService
{
    Task<ReceptionistResponse?> CreateReceptionistAsync(CreateReceptionistRequest request);
    Task<ReceptionistResponse?> GetReceptionistByIdAsync(int id);
    Task<List<ReceptionistResponse>> GetAllReceptionistsAsync();
    Task<ReceptionistResponse?> UpdateReceptionistAsync(int id, UpdateReceptionistRequest request);
    Task<bool> DeleteReceptionistAsync(int id);
}
