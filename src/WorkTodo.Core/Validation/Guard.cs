namespace WorkTodo.Core.Validation;

internal static class Guard
{
    public static string RequiredText(string? value, int maxLength, string paramName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return EnsureMaxLength(value.Trim(), maxLength, paramName);
    }

    public static string? OptionalText(string? value, int maxLength, string paramName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : EnsureMaxLength(value.Trim(), maxLength, paramName);
    }

    private static string EnsureMaxLength(string value, int maxLength, string paramName)
    {
        return value.Length > maxLength
            ? throw new ArgumentException($"Value cannot be longer than {maxLength} characters.", paramName)
            : value;
    }
}
