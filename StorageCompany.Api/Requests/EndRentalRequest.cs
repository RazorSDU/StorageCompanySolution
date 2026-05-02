namespace StorageCompany.Api.Requests;

public class EndRentalRequest
{
    public DateTime EndDateUtc { get; set; } = DateTime.UtcNow;
}
