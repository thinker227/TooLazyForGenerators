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
        
        for (var i = 0; i < NewlineChars.Length; i++)
        {
            if (lastChar == NewlineChars[i]) return true;
        }

        return false;
    }
}
