namespace Ntigra.Services;

public interface IAuditService
{
    void AddAuditLog(string action, string entityType, int entityId, string? details = null);
}
