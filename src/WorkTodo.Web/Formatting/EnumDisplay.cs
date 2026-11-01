using System.Text.RegularExpressions;

namespace WorkTodo.Web.Formatting;

public static partial class EnumDisplay
{
    public static string Text<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        var spaced = CapitalLetter().Replace(value.ToString(), " $1");
        return char.ToUpperInvariant(spaced[0]) + spaced[1..].ToLowerInvariant();
    }

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex CapitalLetter();
}
