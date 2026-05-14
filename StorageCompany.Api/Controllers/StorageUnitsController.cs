using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/storage-units")]
public class StorageUnitsController : ControllerBase
{
    private readonly IStorageUnitService _storageUnits;
    private readonly ISecurityService _security;

    public StorageUnitsController(
        IStorageUnitService storageUnits,
        ISecurityService security)
    {
        _storageUnits = storageUnits;
        _security = security;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StorageUnitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StorageUnitResponse>>> GetAll([FromHeader] string authorization)
    {
        _security.VerifyJwtOrThrow(authorization);
        var units = await _storageUnits.GetAllAsync();
        return Ok(units.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StorageUnitResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StorageUnitResponse>> GetById([FromHeader] string authorization, Guid id)
    {
        _security.VerifyJwtOrThrow(authorization);
        var unit = await _storageUnits.GetByIdAsync(id);
        return Ok(unit.ToResponse());
    }

    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<StorageUnitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StorageUnitResponse>>> GetAvailable(
        [FromHeader] string authorization,
        [FromQuery] Guid? facilityId = null,
        [FromQuery] Guid? unitTypeId = null,
        [FromQuery] decimal? maxPrice = null)
    {
        _security.VerifyJwtOrThrow(authorization);
        var units = await _storageUnits.GetAvailableAsync(facilityId, unitTypeId, maxPrice);
        return Ok(units.Select(x => x.ToResponse()));
    }
}
