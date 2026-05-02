using StorageCompany.Api.Responses;
using StorageCompany.Core.Entities;

namespace StorageCompany.Api.Extensions;

public static class ResponseMappingExtensions
{
    public static CustomerResponse ToResponse(this Customer customer) => new(
        customer.Id,
        customer.FirstName,
        customer.LastName,
        customer.Email,
        customer.PhoneNumber,
        customer.IsActive,
        customer.CreatedAtUtc);

    public static FacilityResponse ToResponse(this Facility facility) => new(
        facility.Id,
        facility.Name,
        facility.Address,
        facility.City,
        facility.PostalCode,
        facility.Country,
        facility.PhoneNumber,
        facility.Email,
        facility.AccessHours,
        facility.OfficeHours,
        facility.HasParking,
        facility.HasElevator,
        facility.HasCCTV);

    public static StorageUnitTypeResponse ToResponse(this StorageUnitType unitType) => new(
        unitType.Id,
        unitType.Name,
        unitType.SizeInSquareMeters,
        unitType.Description,
        unitType.RecommendedFor);

    public static StorageUnitResponse ToResponse(this StorageUnit unit) => new(
        unit.Id,
        unit.FacilityId,
        unit.UnitTypeId,
        unit.UnitNumber,
        unit.Floor,
        unit.MonthlyPrice,
        unit.Status.ToString(),
        unit.IsClimateControlled,
        unit.IsDriveUp);

    public static ReservationResponse ToResponse(this Reservation reservation) => new(
        reservation.Id,
        reservation.CustomerId,
        reservation.StorageUnitId,
        reservation.ReservationDateUtc,
        reservation.MoveInDateUtc,
        reservation.ExpiresAtUtc,
        reservation.Status.ToString());

    public static RentalResponse ToResponse(this Rental rental) => new(
        rental.Id,
        rental.CustomerId,
        rental.StorageUnitId,
        rental.StartDateUtc,
        rental.EndDateUtc,
        rental.MonthlyPrice,
        rental.Status.ToString());

    public static PaymentResponse ToResponse(this Payment payment) => new(
        payment.Id,
        payment.RentalId,
        payment.CustomerId,
        payment.InvoiceId,
        payment.Amount,
        payment.PaymentDateUtc,
        payment.PaymentMethod.ToString(),
        payment.Status.ToString(),
        payment.TransactionReference);

    public static InvoiceResponse ToResponse(this Invoice invoice) => new(
        invoice.Id,
        invoice.RentalId,
        invoice.CustomerId,
        invoice.InvoiceNumber,
        invoice.Amount,
        invoice.DueDateUtc,
        invoice.Status.ToString());

    public static AccessCodeResponse ToResponse(this AccessCode accessCode) => new(
        accessCode.Id,
        accessCode.RentalId,
        accessCode.Code,
        accessCode.IsActive,
        accessCode.CreatedAtUtc,
        accessCode.ExpiresAtUtc);

    public static SupportRequestResponse ToResponse(this SupportRequest supportRequest) => new(
        supportRequest.Id,
        supportRequest.CustomerId,
        supportRequest.RentalId,
        supportRequest.Subject,
        supportRequest.Message,
        supportRequest.Status.ToString(),
        supportRequest.CreatedAtUtc);
}
