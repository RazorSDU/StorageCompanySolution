using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Requests;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservations;

    public ReservationsController(IReservationService reservations)
    {
        _reservations = reservations;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReservationResponse>> GetById(Guid id)
    {
        var reservation = await _reservations.GetByIdAsync(id);
        return Ok(reservation.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReservationResponse>> Create(CreateReservationRequest request)
    {
        var reservation = await _reservations.CreateAsync(request.CustomerId, request.StorageUnitId, request.MoveInDateUtc);
        return CreatedAtAction(nameof(GetById), new { id = reservation.Id }, reservation.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _reservations.CancelAsync(id);
        return NoContent();
    }
}
