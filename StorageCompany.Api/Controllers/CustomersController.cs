using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Requests;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customers;
    private readonly IRentalService _rentals;
    private readonly IReservationService _reservations;
    private readonly IPaymentService _payments;
    private readonly IInvoiceService _invoices;
    private readonly ISupportRequestService _supportRequests;

    public CustomersController(
        ICustomerService customers,
        IRentalService rentals,
        IReservationService reservations,
        IPaymentService payments,
        IInvoiceService invoices,
        ISupportRequestService supportRequests)
    {
        _customers = customers;
        _rentals = rentals;
        _reservations = reservations;
        _payments = payments;
        _invoices = invoices;
        _supportRequests = supportRequests;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetAll()
    {
        var customers = await _customers.GetAllAsync();
        return Ok(customers.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id)
    {
        var customer = await _customers.GetByIdAsync(id);
        return Ok(customer.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request)
    {
        var customer = await _customers.CreateAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Password);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer.ToResponse());
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CustomerResponse>> Update(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _customers.UpdateAsync(id, request.FirstName, request.LastName, request.PhoneNumber, request.IsActive);
        return Ok(customer.ToResponse());
    }

    [HttpGet("{customerId:guid}/rentals")]
    [ProducesResponseType(typeof(IEnumerable<RentalResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RentalResponse>>> GetRentals(Guid customerId)
    {
        var rentals = await _rentals.GetByCustomerIdAsync(customerId);
        return Ok(rentals.Select(x => x.ToResponse()));
    }

    [HttpGet("{customerId:guid}/reservations")]
    [ProducesResponseType(typeof(IEnumerable<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetReservations(Guid customerId)
    {
        var reservations = await _reservations.GetByCustomerIdAsync(customerId);
        return Ok(reservations.Select(x => x.ToResponse()));
    }

    [HttpGet("{customerId:guid}/payments")]
    [ProducesResponseType(typeof(IEnumerable<PaymentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentResponse>>> GetPayments(Guid customerId)
    {
        var payments = await _payments.GetByCustomerIdAsync(customerId);
        return Ok(payments.Select(x => x.ToResponse()));
    }

    [HttpGet("{customerId:guid}/invoices")]
    [ProducesResponseType(typeof(IEnumerable<InvoiceResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<InvoiceResponse>>> GetInvoices(Guid customerId)
    {
        var invoices = await _invoices.GetByCustomerIdAsync(customerId);
        return Ok(invoices.Select(x => x.ToResponse()));
    }

    [HttpGet("{customerId:guid}/support-requests")]
    [ProducesResponseType(typeof(IEnumerable<SupportRequestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SupportRequestResponse>>> GetSupportRequests(Guid customerId)
    {
        var supportRequests = await _supportRequests.GetByCustomerIdAsync(customerId);
        return Ok(supportRequests.Select(x => x.ToResponse()));
    }
}
