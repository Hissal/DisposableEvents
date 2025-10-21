namespace DisposableEvents;

public static partial class FuncResultExtensions {
    public static IEnumerable<TValue> GetValues<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        foreach (var result in results) {
            if (result.HasValue) {
                yield return result.Value;
            }
        }
    }

    public static IEnumerable<TValue?> GetValuesOrDefault<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        foreach (var result in results) {
            yield return result.HasValue ? result.Value : default;
        }
    }

    public static IEnumerable<TValue> GetValuesOrDefault<TValue>(this IEnumerable<FuncResult<TValue>> results,
        TValue defaultValue) {
        foreach (var result in results) {
            yield return result.HasValue ? result.Value : defaultValue;
        }
    }
}