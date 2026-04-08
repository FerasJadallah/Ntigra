using System.ComponentModel.DataAnnotations;

namespace Ntigra.DTOs;

public class UpdateReceptionistRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string? Department { get; set; }

    public DateTime? HireDate { get; set; }

    public decimal? Salary { get; set; }

    public string DeskNumber { get; set; } = string.Empty;
}
