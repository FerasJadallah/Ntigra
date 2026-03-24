using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ntigra.DTOs;
using Ntigra.Services;

namespace Ntigra.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]  // ONLY ADMIN CAN ACCESS RECEPTIONISTS
public class ReceptionistsController : ControllerBase
{
    private readonly IReceptionistService _receptionistService;
    private readonly ILogger<ReceptionistsController> _logger;

    public ReceptionistsController(
        IReceptionistService receptionistService,
        ILogger<ReceptionistsController> logger)
    {
        _receptionistService = receptionistService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateReceptionist(CreateReceptionistRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _receptionistService.CreateReceptionistAsync(request);

        if (result == null)
            return BadRequest(new { message = "Failed to create receptionist. Email or username may already exist." });

        return CreatedAtAction(nameof(GetReceptionistById), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceptionistById(int id)
    {
        var result = await _receptionistService.GetReceptionistByIdAsync(id);

        if (result == null)
            return NotFound(new { message = "Receptionist not found" });

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReceptionists()
    {
        var result = await _receptionistService.GetAllReceptionistsAsync();
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReceptionist(int id, UpdateReceptionistRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _receptionistService.UpdateReceptionistAsync(id, request);

        if (result == null)
            return NotFound(new { message = "Receptionist not found" });

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReceptionist(int id)
    {
        var result = await _receptionistService.DeleteReceptionistAsync(id);

        if (!result)
            return NotFound(new { message = "Receptionist not found" });

        return NoContent();
    }
}
