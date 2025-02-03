namespace Shared.Extensions;

public static class StringExtensions
{
    public static string Format(this string str, params object[] parameters)
    {
        return string.Format(str, parameters);
    }
}
