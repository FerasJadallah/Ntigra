namespace Ntigra.DTOs;

public class EmployeeResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
