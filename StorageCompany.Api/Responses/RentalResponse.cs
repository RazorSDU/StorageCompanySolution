namespace StorageCompany.Api.Responses;

public record RentalResponse(
    Guid Id,
    Guid CustomerId,
    Guid StorageUnitId,
    DateTime StartDateUtc,
    DateTime? EndDateUtc,
    decimal MonthlyPrice,
    string Status);
