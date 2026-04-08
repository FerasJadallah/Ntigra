using System.ComponentModel.DataAnnotations;

namespace Ntigra.DTOs;

public class CreateEmployeeRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Username is required")]
    [MinLength(3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? Department { get; set; }

    public DateTime? HireDate { get; set; }

    public decimal? Salary { get; set; }
}
