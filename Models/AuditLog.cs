using System.ComponentModel.DataAnnotations;

namespace Ntigra.Models;

public class AuditLog
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }  // Who did it
    
    [Required]
    public string Action { get; set; } = string.Empty;  // "CREATE", "UPDATE", "DELETE"
    
    [Required]
    public string EntityType { get; set; } = string.Empty;  // "Patient"
    
    [Required]
    public int EntityId { get; set; }  // Which patient
    
    public string? Details { get; set; }  // JSON of changes (for updates)
    
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public User? User { get; set; }
}
