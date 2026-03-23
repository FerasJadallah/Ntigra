using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ntigra.DTOs;
using Ntigra.Services;

namespace Ntigra.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly ILogger<PatientsController> _logger;

    public PatientsController(IPatientService patientService, ILogger<PatientsController> logger)
    {
        _patientService = patientService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Receptionist,Admin")]
    public async Task<IActionResult> CreatePatient(CreatePatientRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _patientService.CreatePatientAsync(request);

        if (result == null)
            return BadRequest(new { message = "Failed to create patient. Email or username may already exist." });

        return CreatedAtAction(nameof(GetPatientById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Receptionist,Patient,Admin")]
    public async Task<IActionResult> GetPatientById(int id)
    {
        var result = await _patientService.GetPatientByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Patient not found" });

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Receptionist,Admin")]
    public async Task<IActionResult> GetAllPatients()
    {
        var result = await _patientService.GetAllPatientsAsync();
        return Ok(result);
    }

[HttpPut("{id}")]
[Authorize(Roles = "Receptionist,Admin")]  
public async Task<IActionResult> UpdatePatient(int id, UpdatePatientRequest request)  
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    var result = await _patientService.UpdatePatientAsync(id, request);

    if (result == null)
        return NotFound(new { message = "Patient not found" });

    return Ok(result);
}

    [HttpDelete("{id}")]
    [Authorize(Roles = "Receptionist,Admin")]
    public async Task<IActionResult> DeletePatient(int id)
    {
        var result = await _patientService.DeletePatientAsync(id);

        if (!result)
            return NotFound(new { message = "Patient not found" });

        return NoContent();
    }
}
