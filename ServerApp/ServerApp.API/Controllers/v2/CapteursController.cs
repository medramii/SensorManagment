using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Contracts.Logging;
using ServerApp.Contracts.Repositories;
using ServerApp.Domain.Data;
using ServerApp.Domain.Models;

namespace ServerApp.API.Controllers.v2;

/// <summary>
/// This version of api interacts with an PostgreSQL Database
/// </summary>
///
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class CapteursController : ControllerBase
{
    private readonly ICapteurRepository<PostgresDbContext> _capteurRepo;
    private readonly ILoggerManager _logger;

    public CapteursController(ICapteurRepository<PostgresDbContext> capteurRepo, ILoggerManager logger)
    {
        _capteurRepo = capteurRepo;
        _logger = logger;
    }

    /// <summary>
    /// Get all capteurs
    /// </summary>
    /// <returns>List of capteurs</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Capteur>>> GetCapteurs()
    {
        _logger.LogDebug($"Attempting to retrieve all capteurs.");

        try
        {
            var _capteursCriteria = await _capteurRepo.GetAllAsync();

            if (_capteursCriteria == null || !_capteursCriteria.Any())
            {
                _logger.LogWarn($"No capteurs found.");
                return NoContent();
            }

            var result = _capteursCriteria.Select(o => new
            {
                o.Id,
                o.Label,
                o.Type,
                o.Active,
                o.CreatedAt
            });

            _logger.LogInfo($"Successfully retrieved {result.Count()} capteurs.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving capteurs.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get capteur by ID
    /// </summary>
    /// <param name="id">Capteur ID</param>
    /// <returns>Specific capteur</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Capteur>> GetCapteur(int id)
    {
        _logger.LogDebug($"Attempting to retrieve capteur with ID {id}.");

        try
        {
            var capteur = await _capteurRepo.FindAsync(id);

            if (capteur == null)
            {
                _logger.LogWarn($"Capteur with ID {id} not found.");
                return NotFound($"Capteur with ID {id} not found.");
            }

            _logger.LogInfo($"Successfully retrieved capteur with ID {id}.");
            return Ok(capteur);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving capteur with ID {id}.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a capteur
    /// </summary>
    /// <param name="id">Capteur ID</param>
    /// <param name="capteur">Capteur object</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCapteur(int id, Capteur capteur)
    {
        _logger.LogDebug($"Attempting to update capteur with ID {id}.");

        if (id != capteur.Id)
        {
            _logger.LogWarn($"Mismatched ID: URL ID {id} does not match capteur ID {capteur.Id}.");
            return BadRequest("Capteur ID mismatch.");
        }

        try
        {
            await _capteurRepo.UpdateAsync(capteur);
            await _capteurRepo.SaveAsync();
            _logger.LogInfo($"Successfully updated capteur with ID {id}.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, $"Concurrency error when updating capteur with ID {id}.");
            return StatusCode(409, "Concurrency conflict occurred.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating capteur with ID {id}.");
            return StatusCode(500, "Internal server error");
        }

        return NoContent();
    }

    /// <summary>
    /// Create a new capteur
    /// </summary>
    /// <param name="capteur">Capteur object</param>
    /// <returns>Created capteur</returns>
    [HttpPost]
    public async Task<ActionResult<Capteur>> PostCapteur(Capteur capteur)
    {
        _logger.LogDebug($"Attempting to create a new capteur.");

        try
        {
            if (_capteurRepo == null)
            {
                _logger.LogError("Capteur repository is null.");
                return Problem("Capteur repository is null.");
            }

            capteur.Active = true;
            capteur.CreatedAt = DateTime.Now;

            await _capteurRepo.CreateAsync(capteur);
            await _capteurRepo.SaveAsync();
            _logger.LogInfo($"Successfully created capteur with ID {capteur.Id}.");

            return CreatedAtAction("GetCapteur", new { id = capteur.Id }, capteur);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while creating a new capteur.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a capteur by ID
    /// </summary>
    /// <param name="id">Capteur ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCapteur(int id)
    {
        _logger.LogDebug($"Attempting to delete capteur with ID {id}.");

        try
        {
            var capteur = await _capteurRepo.FindAsync(id);

            if (capteur == null)
            {
                _logger.LogWarn($"Capteur with ID {id} not found.");
                return NotFound($"Capteur with ID {id} not found.");
            }

            await _capteurRepo.DeleteAsync(capteur);
            await _capteurRepo.SaveAsync();
            _logger.LogInfo($"Successfully deleted capteur with ID {id}.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting capteur with ID {id}.");
            return StatusCode(500, "Internal server error");
        }
    }
}
