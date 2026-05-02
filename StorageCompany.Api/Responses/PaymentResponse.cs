namespace StorageCompany.Api.Responses;

public record PaymentResponse(
    Guid Id,
    Guid RentalId,
    Guid CustomerId,
    Guid? InvoiceId,
    decimal Amount,
    DateTime PaymentDateUtc,
    string PaymentMethod,
    string Status,
    string TransactionReference);
