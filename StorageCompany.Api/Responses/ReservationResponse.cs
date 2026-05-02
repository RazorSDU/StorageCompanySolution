namespace StorageCompany.Api.Responses;

public record ReservationResponse(
    Guid Id,
    Guid CustomerId,
    Guid StorageUnitId,
    DateTime ReservationDateUtc,
    DateTime MoveInDateUtc,
    DateTime? ExpiresAtUtc,
    string Status);
