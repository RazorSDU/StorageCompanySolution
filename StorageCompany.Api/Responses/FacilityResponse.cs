namespace StorageCompany.Api.Responses;

public record FacilityResponse(
    Guid Id,
    string Name,
    string Address,
    string City,
    string PostalCode,
    string Country,
    string PhoneNumber,
    string Email,
    string AccessHours,
    string OfficeHours,
    bool HasParking,
    bool HasElevator,
    bool HasCCTV);
