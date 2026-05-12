using System.ComponentModel.DataAnnotations;

namespace StorageCompany.Core.Enums;

public static class Constants
{
    [Required] public static string CustomerRole = "customer";

    [Required] public static string AdminRole = "admin";
}