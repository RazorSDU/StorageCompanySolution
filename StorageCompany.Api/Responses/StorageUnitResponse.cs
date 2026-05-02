namespace StorageCompany.Api.Responses;

public record StorageUnitResponse(
    Guid Id,
    Guid FacilityId,
    Guid UnitTypeId,
    string UnitNumber,
    int Floor,
    decimal MonthlyPrice,
    string Status,
    bool IsClimateControlled,
    bool IsDriveUp);
