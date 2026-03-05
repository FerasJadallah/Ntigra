using Ntigra.Data;
using Ntigra.Models;

namespace Ntigra.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        AppDbContext context,
        ICurrentUserService currentUserService,
        ILogger<AuditService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public void AddAuditLog(string action, string entityType, int entityId, string? details = null)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            _logger.LogWarning(
                "Audit log skipped because current user id is missing. Action: {Action}, EntityType: {EntityType}, EntityId: {EntityId}",
                action,
                entityType,
                entityId);
            return;
        }

        var log = new AuditLog
        {
            UserId = userId.Value,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
    }
}
