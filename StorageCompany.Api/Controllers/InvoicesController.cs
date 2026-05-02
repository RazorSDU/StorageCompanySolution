using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Requests;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoices;

    public InvoicesController(IInvoiceService invoices)
    {
        _invoices = invoices;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InvoiceResponse>> GetById(Guid id)
    {
        var invoice = await _invoices.GetByIdAsync(id);
        return Ok(invoice.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<InvoiceResponse>> Generate(GenerateInvoiceRequest request)
    {
        var invoice = await _invoices.GenerateForRentalAsync(request.RentalId, request.DueDateUtc);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id }, invoice.ToResponse());
    }

    [HttpPut("{id:guid}/mark-paid")]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<InvoiceResponse>> MarkAsPaid(Guid id)
    {
        var invoice = await _invoices.MarkAsPaidAsync(id);
        return Ok(invoice.ToResponse());
    }
}
