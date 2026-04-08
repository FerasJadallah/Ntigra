using System.ComponentModel.DataAnnotations;

namespace Ntigra.Models;

public class Employee : User
{
    [Required]
    public string EmployeeId { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? Department { get; set; }

    public DateTime? HireDate { get; set; }

    public decimal? Salary { get; set; }
}
