namespace Ntigra.DTOs;

public class CreateReceptionistRequest : CreateEmployeeRequest
{
    public string DeskNumber { get; set; } = string.Empty;
}
