using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Requests;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _payments;

    public PaymentsController(IPaymentService payments)
    {
        _payments = payments;
    }


    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentResponse>> GetById(Guid id)
    {
        var payment = await _payments.GetByIdAsync(id);
        return Ok(payment.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PaymentResponse>> Create(CreatePaymentRequest request)
    {
        var payment = await _payments.CreateMockPaymentAsync(
            request.RentalId,
            request.Amount,
            request.PaymentMethod,
            request.InvoiceId);

        return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment.ToResponse());
    }
}
