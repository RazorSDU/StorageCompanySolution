namespace StorageCompany.Core.Entities;

public class Facility : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AccessHours { get; set; } = string.Empty;
    public string OfficeHours { get; set; } = string.Empty;
    public bool HasParking { get; set; }
    public bool HasElevator { get; set; }
    public bool HasCCTV { get; set; }
}
