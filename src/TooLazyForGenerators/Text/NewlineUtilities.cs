using System.Text;

namespace TooLazyForGenerators.Text;

internal static class NewlineUtilities
{
    public const string NewlineChars = "\n\r\f\u0085\u2028\u2029";

    public static bool EndsWithNewline(this ReadOnlySpan<char> span)
    {
        if (span.IsEmpty) return false;
        
        for (var i = 0; i < NewlineChars.Length; i++)
        {
            if (span.EndsWith(NewlineChars.AsSpan(i, 1))) return true;
        }

        return false;
    }

    public static bool EndsWithNewline(this StringBuilder builder)
    {
        if (builder.Length == 0) return false;
        
        var lastChar = builder[^1];
        
        foreach (var t in NewlineChars)
        {
            if (lastChar == t) return true;
        }

        return false;
    }

    public static bool IsOnlyNewlines(this ReadOnlySpan<char> span)
    {
        var newlines = NewlineChars.AsSpan();
        
        foreach (var c in span)
        {
            if (newlines.IndexOf(c) == -1) return false;
        }

        return true;
    }

    public static bool ContainsNewline(this ReadOnlySpan<char> span) =>
        span.IndexOfAny(NewlineChars.AsSpan()) != -1;
}
