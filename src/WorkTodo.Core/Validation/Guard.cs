namespace WorkTodo.Core.Validation;

internal static class Guard
{
    public static string RequiredText(string? value, int maxLength, string fieldLabel)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{fieldLabel} is required.");
        }

        return EnsureMaxLength(value.Trim(), maxLength, fieldLabel);
    }

    public static string? OptionalText(string? value, int maxLength, string fieldLabel)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : EnsureMaxLength(value.Trim(), maxLength, fieldLabel);
    }

    public static TEnum DefinedEnum<TEnum>(TEnum value, string fieldLabel)
        where TEnum : struct, Enum
    {
        return Enum.IsDefined(value)
            ? value
            : throw new ArgumentException($"{fieldLabel} is not a valid value.");
    }

    private static string EnsureMaxLength(string value, int maxLength, string fieldLabel)
    {
        return value.Length > maxLength
            ? throw new ArgumentException($"{fieldLabel} cannot be longer than {maxLength} characters.")
            : value;
    }
}
