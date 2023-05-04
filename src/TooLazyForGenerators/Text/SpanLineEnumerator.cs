namespace TooLazyForGenerators.Text;

// Adapted from source:
// https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/Text/SpanLineEnumerator.cs

/// <summary>
/// Enumerates the lines of a <see cref="ReadOnlySpan{Char}"/>.
/// </summary>
internal ref struct SpanLineEnumerator
{
    private const string NewlineChars = "\n\r\f\u0085\u2028\u2029";
    private ReadOnlySpan<char> remaining;
    private ReadOnlySpan<char> current;
    private bool isEnumeratorActive;

    internal SpanLineEnumerator(ReadOnlySpan<char> buffer)
    {
        remaining = buffer;
        current = default;
        isEnumeratorActive = true;
    }

    /// <summary>
    /// Gets the line at the current position of the enumerator.
    /// </summary>
    public ReadOnlySpan<char> Current => current;

    /// <summary>
    /// Returns this instance as an enumerator.
    /// </summary>
    public SpanLineEnumerator GetEnumerator() => this;

    /// <summary>
    /// Advances the enumerator to the next line of the span.
    /// </summary>
    /// <returns>
    /// True if the enumerator successfully advanced to the next line; false if
    /// the enumerator has advanced past the end of the span.
    /// </returns>
    public bool MoveNext()
    {
        if (!isEnumeratorActive)
        {
            return false; // EOF previously reached or enumerator was never initialized
        }

        var remaining = this.remaining;

        var idx = remaining.IndexOfAny(NewlineChars.AsSpan());

        if ((uint)idx < (uint)remaining.Length)
        {
            int stride = 1;

            if (remaining[idx] == '\r' && (uint)(idx + 1) < (uint)remaining.Length && remaining[idx + 1] == '\n')
            {
                stride = 2;
            }

            // current = remaining.Slice(0, idx);
            // Include the newline character!
            current = remaining.Slice(0, idx + stride);
            this.remaining = remaining.Slice(idx + stride);
        }
        else
        {
            // We've reached EOF, but we still need to return 'true' for this final
            // iteration so that the caller can query the Current property once more.

            current = remaining;
            this.remaining = default;
            isEnumeratorActive = false;
        }

        return true;
    }
}

internal static class SpanExtensions
{
    public static SpanLineEnumerator EnumerateLines(this ReadOnlySpan<char> span) => new(span);
}
