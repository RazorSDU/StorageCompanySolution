using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Api.Requests;

public class UpdateCustomerRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
