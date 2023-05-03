namespace TooLazyForGenerators;

/// <summary>
/// Extensions for compatibility between .NET Framework 4.7.2, .NET Core 3.1, and the latest language version.
/// </summary>
internal static class CompatibilityExtensions
{
    public static void Deconstruct<TKey, TValue>(
        this KeyValuePair<TKey, TValue> pair,
        out TKey key,
        out TValue value)
    {
        key = pair.Key;
        value = pair.Value;
    }
}
