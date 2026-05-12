using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Core.Entities;

public class AuthResponse
{
    [Required] public string Jwt { get; set; } = null!;
}