namespace Ntigra.DTOs;

public class UpdateReceptionistRequest : UpdateEmployeeRequest
{
    public string DeskNumber { get; set; } = string.Empty;
}
