using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerApp.Contracts.Logging;
using ServerApp.Contracts.Repositories;
using ServerApp.Domain.Models;

namespace ServerApp.API.Controllers.v2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepository _ticketRepo;
    private readonly ILoggerManager _logger;

    public TicketsController(ITicketRepository ticketRepo, ILoggerManager logger)
    {
        _ticketRepo = ticketRepo;
        _logger = logger;
    }

    /// <summary>
    /// Get all tickets
    /// </summary>
    /// <returns>List of tickets</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Ticket>>> GetTickets()
    {
        _logger.LogDebug($"Attempting to retrieve all tickets.");

        try
        {
            var _ticketsCriteria = await _ticketRepo.GetAllAsync();

            if (_ticketsCriteria == null || !_ticketsCriteria.Any())
            {
                _logger.LogWarn($"No tickets found.");
                return NoContent();
            }

            var result = _ticketsCriteria.Select(o => new
            {
                o.Id,
                o.Description,
                o.Status,
                o.CreatedAt
            });

            _logger.LogInfo($"Successfully retrieved {result.Count()} tickets.");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving tickets.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get ticket by ID
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <returns>Specific ticket</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<Ticket>> GetTicket(int id)
    {
        _logger.LogDebug($"Attempting to retrieve ticket with ID {id}.");

        try
        {
            var ticket = await _ticketRepo.FindAsync(id);

            if (ticket == null)
            {
                _logger.LogWarn($"Ticket with ID {id} not found.");
                return NotFound($"Ticket with ID {id} not found.");
            }

            _logger.LogInfo($"Successfully retrieved ticket with ID {id}.");
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving ticket with ID {id}.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update a ticket
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <param name="ticket">Ticket object</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutTicket(int id, Ticket ticket)
    {
        _logger.LogDebug($"Attempting to update ticket with ID {id}.");

        if (id != ticket.Id)
        {
            _logger.LogWarn($"Mismatched ID: URL ID {id} does not match ticket ID {ticket.Id}.");
            return BadRequest("Ticket ID mismatch.");
        }

        try
        {
            await _ticketRepo.UpdateAsync(ticket);
            await _ticketRepo.SaveAsync();
            _logger.LogInfo($"Successfully updated ticket with ID {id}.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, $"Concurrency error when updating ticket with ID {id}.");
            return StatusCode(409, "Concurrency conflict occurred.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while updating ticket with ID {id}.");
            return StatusCode(500, "Internal server error");
        }

        return NoContent();
    }

    /// <summary>
    /// Create a new ticket
    /// </summary>
    /// <param name="ticket">Ticket object</param>
    /// <returns>Created ticket</returns>
    [HttpPost]
    public async Task<ActionResult<Ticket>> PostTicket(Ticket ticket)
    {
        _logger.LogDebug($"Attempting to create a new ticket.");

        try
        {
            if (_ticketRepo == null)
            {
                _logger.LogError("Ticket repository is null.");
                return Problem("Ticket repository is null.");
            }

            await _ticketRepo.CreateAsync(ticket);
            await _ticketRepo.SaveAsync();
            _logger.LogInfo($"Successfully created ticket with ID {ticket.Id}.");

            return CreatedAtAction("GetTicket", new { id = ticket.Id }, ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while creating a new ticket.");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a ticket by ID
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        _logger.LogDebug($"Attempting to delete ticket with ID {id}.");

        try
        {
            var ticket = await _ticketRepo.FindAsync(id);

            if (ticket == null)
            {
                _logger.LogWarn($"Ticket with ID {id} not found.");
                return NotFound($"Ticket with ID {id} not found.");
            }

            await _ticketRepo.DeleteAsync(ticket);
            await _ticketRepo.SaveAsync();
            _logger.LogInfo($"Successfully deleted ticket with ID {id}.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while deleting ticket with ID {id}.");
            return StatusCode(500, "Internal server error");
        }
    }
}
