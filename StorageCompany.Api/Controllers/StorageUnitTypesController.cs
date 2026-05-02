using Microsoft.AspNetCore.Mvc;
using StorageCompany.Api.Extensions;
using StorageCompany.Api.Responses;
using StorageCompany.Core.Interfaces.Services;

namespace StorageCompany.Api.Controllers;

[ApiController]
[Route("api/storage-unit-types")]
public class StorageUnitTypesController : ControllerBase
{
    private readonly IStorageUnitTypeService _unitTypes;

    public StorageUnitTypesController(IStorageUnitTypeService unitTypes)
    {
        _unitTypes = unitTypes;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StorageUnitTypeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StorageUnitTypeResponse>>> GetAll()
    {
        var unitTypes = await _unitTypes.GetAllAsync();
        return Ok(unitTypes.Select(x => x.ToResponse()));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StorageUnitTypeResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StorageUnitTypeResponse>> GetById(Guid id)
    {
        var unitType = await _unitTypes.GetByIdAsync(id);
        return Ok(unitType.ToResponse());
    }
}
