using System.ComponentModel.DataAnnotations;

namespace Ntigra.Models;

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
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = string.Empty;  // No default!
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}