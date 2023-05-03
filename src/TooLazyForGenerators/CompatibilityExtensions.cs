namespace TooLazyForGenerators;

/// <summary>
/// Extensions for compatibility between .NET Standard 2.0 and the latest language version.
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
