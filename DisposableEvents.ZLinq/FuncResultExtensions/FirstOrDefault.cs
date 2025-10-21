using ZLinq;

namespace DisposableEvents.ZLinq;

public static partial class FuncResultExtensions {
    public static TValue? FirstValueOrDefault<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results) where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> {
        using var enumerator = results.Enumerator;
        while (enumerator.TryGetNext(out var result)) {
            if (result.HasValue) {
                return result.Value;
            }
        }
        return default;
    }
    public static TValue FirstValueOrDefault<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results, TValue defaultValue) where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> {
        using var enumerator = results.Enumerator;
        while (enumerator.TryGetNext(out var result)) {
            if (result.HasValue) {
                return result.Value;
            }
        }
        return defaultValue;
    }
}