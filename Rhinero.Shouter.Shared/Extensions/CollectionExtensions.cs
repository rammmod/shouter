namespace Rhinero.Shouter.Shared.Extensions
{
    internal static class CollectionExtensions
    {
        internal static bool IsNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) =>
            dictionary is null || dictionary.Count == 0;

        internal static bool NotNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) =>
            dictionary is not null && dictionary.Any();
    }
}
