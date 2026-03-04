using System.ComponentModel.DataAnnotations;

namespace Ntigra.Models;

// This class represents a user in the system, with properties for email, password hash, and creation date.

public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Patient"; // Default role is "Patient"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
