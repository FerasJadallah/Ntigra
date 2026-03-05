using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ntigra.Data;

namespace Ntigra.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Receptionist")]  // Only receptionists can view audits
public class AuditController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditController> _logger;

    public AuditController(AppDbContext context, ILogger<AuditController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Audit
    [HttpGet]
    public async Task<IActionResult> GetAllAuditLogs()
    {
        try
        {
            var logs = await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new
                {
                    a.Id,
                    a.Action,
                    a.EntityType,
                    a.EntityId,
                    a.Details,
                    a.Timestamp,
                    UserEmail = a.User != null ? a.User.Email : null,
                    UserId = a.UserId
                })
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs");
            return StatusCode(500, new { message = "Error retrieving audit logs" });
        }
    }

    // GET: api/Audit/patient/5
    [HttpGet("patient/{patientId}")]
    public async Task<IActionResult> GetAuditLogsForPatient(int patientId)
    {
        try
        {
            var logs = await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.EntityType == "Patient" && a.EntityId == patientId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new
                {
                    a.Id,
                    a.Action,
                    a.Details,
                    a.Timestamp,
                    UserEmail = a.User != null ? a.User.Email : null
                })
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for patient {PatientId}", patientId);
            return StatusCode(500, new { message = "Error retrieving audit logs" });
        }
    }

    // GET: api/Audit/user/5
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetAuditLogsByUser(int userId)
    {
        try
        {
            var logs = await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new
                {
                    a.Id,
                    a.Action,
                    a.EntityType,
                    a.EntityId,
                    a.Details,
                    a.Timestamp,
                    UserEmail = a.User != null ? a.User.Email : null
                })
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit logs for user {UserId}", userId);
            return StatusCode(500, new { message = "Error retrieving audit logs" });
        }
    }

    // GET: api/Audit/summary
    [HttpGet("summary")]
    public async Task<IActionResult> GetAuditSummary()
    {
        try
        {
            var summary = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new
                {
                    Action = g.Key,
                    Count = g.Count(),
                    LastOccurrence = g.Max(a => a.Timestamp)
                })
                .ToListAsync();

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting audit summary");
            return StatusCode(500, new { message = "Error retrieving audit summary" });
        }
    }
}
