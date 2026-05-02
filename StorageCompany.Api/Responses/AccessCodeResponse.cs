namespace StorageCompany.Api.Responses;

public record AccessCodeResponse(
    Guid Id,
    Guid RentalId,
    string Code,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc);
