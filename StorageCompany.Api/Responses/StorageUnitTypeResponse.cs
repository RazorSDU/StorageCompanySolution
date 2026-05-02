namespace StorageCompany.Api.Responses;

public record StorageUnitTypeResponse(
    Guid Id,
    string Name,
    decimal SizeInSquareMeters,
    string Description,
    string RecommendedFor);
