using System.ComponentModel.DataAnnotations;

namespace Ntigra.DTOs;

public class UpdatePatientRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    public DateTime DateOfBirth { get; set; }
    
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [EmailAddress]
    public string Email { get; set; } = string.Empty;  // Email can be updated
    
    public string Address { get; set; } = string.Empty;
}
