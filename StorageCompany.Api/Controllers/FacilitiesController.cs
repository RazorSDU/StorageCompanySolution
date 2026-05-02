using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FacilitiesController : ControllerBase
{
    private readonly IFacilityService _facilities;
    private readonly IStorageUnitService _storageUnits;

    public FacilitiesController(IFacilityService facilities, IStorageUnitService storageUnits)
    {
        _facilities = facilities;
        _storageUnits = storageUnits;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FacilityResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FacilityResponse>>> GetAll([FromQuery] string? search = null)
    {
        var facilities = await _facilities.GetAllAsync(search);
        return Ok(facilities.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FacilityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<FacilityResponse>> GetById(Guid id)
    {
        var facility = await _facilities.GetByIdAsync(id);
        return Ok(facility.ToResponse());
    }

    [HttpGet("{id:guid}/units")]
    [ProducesResponseType(typeof(IEnumerable<StorageUnitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StorageUnitResponse>>> GetUnits(Guid id)
    {
        var units = await _storageUnits.GetByFacilityIdAsync(id);
        return Ok(units.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:guid}/available-units")]
    [ProducesResponseType(typeof(IEnumerable<StorageUnitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StorageUnitResponse>>> GetAvailableUnits(Guid id)
    {
        var units = await _storageUnits.GetAvailableAsync(id);
        return Ok(units.Select(x => x.ToResponse()));
    }
}
