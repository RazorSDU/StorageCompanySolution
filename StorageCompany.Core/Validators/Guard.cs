using StorageCompany.Core.Exceptions;

namespace StorageCompany.Core.Validators;

public static class Guard
{
    public static void AgainstEmpty(Guid value, string fieldName)
    {
        if (value == Guid.Empty)
            throw new BusinessRuleException($"{fieldName} cannot be empty.");
    }

    public static void AgainstBlank(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new BusinessRuleException($"{fieldName} cannot be empty.");
    }

    public static void AgainstNonPositive(decimal value, string fieldName)
    {
        if (value <= 0)
            throw new BusinessRuleException($"{fieldName} must be greater than zero.");
    }
}
