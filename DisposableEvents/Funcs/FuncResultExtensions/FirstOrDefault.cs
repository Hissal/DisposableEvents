namespace DisposableEvents;

public static partial class FuncResultExtensions {
    public static TValue? FirstValueOrDefault<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        foreach (var result in results) {
            if (result.HasValue) {
                return result.Value;
            }
        }

        return default;
    }

    public static TValue FirstValueOrDefault<TValue>(this IEnumerable<FuncResult<TValue>> results, TValue defaultValue) {
        foreach (var result in results) {
            if (result.HasValue) {
                return result.Value;
            }
        }

        return defaultValue;
    }
}