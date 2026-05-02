namespace StorageCompany.Api.Responses;

public record InvoiceResponse(
    Guid Id,
    Guid RentalId,
    Guid CustomerId,
    string InvoiceNumber,
    decimal Amount,
    DateTime DueDateUtc,
    string Status);
