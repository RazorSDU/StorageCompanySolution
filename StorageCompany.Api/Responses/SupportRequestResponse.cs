namespace StorageCompany.Api.Responses;

public record SupportRequestResponse(
    Guid Id,
    Guid CustomerId,
    Guid? RentalId,
    string Subject,
    string Message,
    string Status,
    DateTime CreatedAtUtc);
