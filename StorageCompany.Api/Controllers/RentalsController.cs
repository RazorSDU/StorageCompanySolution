using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Requests;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentals;
    private readonly IAccessCodeService _accessCodes;
    private readonly IPaymentService _payments;
    private readonly IInvoiceService _invoices;

    public RentalsController(
        IRentalService rentals,
        IAccessCodeService accessCodes,
        IPaymentService payments,
        IInvoiceService invoices)
    {
        _rentals = rentals;
        _accessCodes = accessCodes;
        _payments = payments;
        _invoices = invoices;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RentalResponse>> GetById(Guid id)
    {
        var rental = await _rentals.GetByIdAsync(id);
        return Ok(rental.ToResponse());
    }

    [HttpPost("from-reservation")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RentalResponse>> CreateFromReservation(CreateRentalFromReservationRequest request)
    {
        var rental = await _rentals.CreateFromReservationAsync(request.ReservationId);
        return CreatedAtAction(nameof(GetById), new { id = rental.Id }, rental.ToResponse());
    }

    [HttpPost("direct")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<RentalResponse>> CreateDirect(CreateDirectRentalRequest request)
    {
        var rental = await _rentals.CreateDirectAsync(request.CustomerId, request.StorageUnitId, request.StartDateUtc);
        return CreatedAtAction(nameof(GetById), new { id = rental.Id }, rental.ToResponse());
    }

    [HttpPut("{id:guid}/end")]
    [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RentalResponse>> EndRental(Guid id, EndRentalRequest request)
    {
        var rental = await _rentals.EndRentalAsync(id, request.EndDateUtc);
        return Ok(rental.ToResponse());
    }

    [HttpGet("{rentalId:guid}/access-code")]
    [ProducesResponseType(typeof(AccessCodeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessCodeResponse>> GetAccessCode(Guid rentalId)
    {
        var code = await _accessCodes.GetActiveByRentalIdAsync(rentalId);
        return Ok(code.ToResponse());
    }

    [HttpGet("{rentalId:guid}/payments")]
    [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetPayments(Guid rentalId)
    {
        var payments = await _payments.GetByRentalIdAsync(rentalId);
        return Ok(payments.Select(x => x.ToResponse()));
    }

    [HttpGet("{rentalId:guid}/invoices")]
    [ProducesResponseType(typeof(IEnumerable<InvoiceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InvoiceResponse>>> GetInvoices(Guid rentalId)
    {
        var invoices = await _invoices.GetByRentalIdAsync(rentalId);
        return Ok(invoices.Select(x => x.ToResponse()));
    }
}
