using System.Text;

namespace TooLazyForGenerators.Text;

internal static class StringBuilderExtensions
{
    public static unsafe StringBuilder Append(this StringBuilder builder, ReadOnlySpan<char> value)
    {
        if (value.Length <= 0) return builder;
        
        fixed (char* valueChars = value)
        {
            builder.Append(valueChars, value.Length);
        }

        return builder;
    }
}
