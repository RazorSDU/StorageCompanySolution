using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Requests;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/support-requests")]
public class SupportRequestsController : ControllerBase
{
    private readonly ISupportRequestService _supportRequests;

    public SupportRequestsController(ISupportRequestService supportRequests)
    {
        _supportRequests = supportRequests;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SupportRequestResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SupportRequestResponse>>> GetAll()
    {
        var requests = await _supportRequests.GetAllAsync();
        return Ok(requests.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupportRequestResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SupportRequestResponse>> GetById(Guid id)
    {
        var request = await _supportRequests.GetByIdAsync(id);
        return Ok(request.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(SupportRequestResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<SupportRequestResponse>> Create(CreateSupportRequestRequest request)
    {
        var supportRequest = await _supportRequests.CreateAsync(
            request.CustomerId,
            request.RentalId,
            request.Subject,
            request.Message);

        return CreatedAtAction(nameof(GetById), new { id = supportRequest.Id }, supportRequest.ToResponse());
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(SupportRequestResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SupportRequestResponse>> UpdateStatus(Guid id, UpdateSupportRequestStatusRequest request)
    {
        var supportRequest = await _supportRequests.UpdateStatusAsync(id, request.Status);
        return Ok(supportRequest.ToResponse());
    }
}
