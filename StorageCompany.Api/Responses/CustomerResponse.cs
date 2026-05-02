namespace StorageCompany.Api.Responses;

public record CustomerResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    bool IsActive,
    DateTime CreatedAtUtc);
