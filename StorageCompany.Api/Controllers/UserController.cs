using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Requests;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _users;
    private readonly IRentalService _rentals;
    private readonly IReservationService _reservations;
    private readonly IPaymentService _payments;
    private readonly IInvoiceService _invoices;
    private readonly ISupportRequestService _supportRequests;
    private readonly ISecurityService _security;

    public UserController(
        IUserService users,
        IRentalService rentals,
        IReservationService reservations,
        IPaymentService payments,
        IInvoiceService invoices,
        ISupportRequestService supportRequests,
        ISecurityService securityService)
    {
        _users = users;
        _rentals = rentals;
        _reservations = reservations;
        _payments = payments;
        _invoices = invoices;
        _supportRequests = supportRequests;
        _security =  securityService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserResponse>>> GetAll([FromHeader] string authorization)
    {
        _security.VerifyJwtOrThrow(authorization);
        
        var customers = await _users.GetAllAsync();
        
        return Ok(customers.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var customer = await _users.GetByIdAsync(id);
        return Ok(customer.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var customer = await _users.CreateAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Password);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer.ToResponse());
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> Update(Guid id, UpdateUserRequest request)
    {
        var customer = await _users.UpdateAsync(id, request.FirstName, request.LastName, request.PhoneNumber, request.IsActive);
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
