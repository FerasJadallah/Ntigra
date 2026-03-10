using System.ComponentModel.DataAnnotations;

namespace Ntigra.Models;

public class Admin : User
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public string? Department { get; set; }
    
    public bool IsSuperAdmin { get; set; } = false;
}
