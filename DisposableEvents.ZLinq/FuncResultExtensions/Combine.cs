using ZLinq;

namespace DisposableEvents.ZLinq;

public static partial class FuncResultExtensionsZLinq {
    public static FuncResult<TValue> Combine<TEnumerator, TValue>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Func<FuncResult<TValue>, FuncResult<TValue>, FuncResult<TValue>> combiner)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>>
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        using var enumerator = results.Enumerator;

        if (!enumerator.TryGetNext(out var combined))
            return FuncResult<TValue>.Null();

        while (enumerator.TryGetNext(out var result)) {
            combined = combiner(combined, result);
        }

        return combined;
    }

    public static FuncResult<TValue> Combine<TState, TEnumerator, TValue>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Func<TState, FuncResult<TValue>, FuncResult<TValue>, FuncResult<TValue>> combiner) 
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;

        if (!enumerator.TryGetNext(out var combined))
            return FuncResult<TValue>.Null();

        while (enumerator.TryGetNext(out var result)) {
            combined = combiner(state, combined, result);
        }

        return combined;
    }

    public static TValue? CombineValues<TEnumerator, TValue>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Func<TValue, TValue, TValue> combiner) 
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;

        while (enumerator.TryGetNext(out var result)) {
            if (result.IsNull)
                continue;

            var combined = result.Value;

            while (enumerator.TryGetNext(out result)) {
                if (result.HasValue) {
                    combined = combiner(combined, result.Value);
                }
            }

            return combined;
        }

        return default;
    }

    public static TValue? CombineValues<TState, TEnumerator, TValue>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Func<TState, TValue, TValue, TValue> combiner)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;

        while (enumerator.TryGetNext(out var result)) {
            if (result.IsNull)
                continue;

            var combined = result.Value;

            while (enumerator.TryGetNext(out result)) {
                if (result.HasValue) {
                    combined = combiner(state, combined, result.Value);
                }
            }

            return combined;
        }

        return default;
    }

    public static TValue CombineValues<TEnumerator, TValue>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Func<TValue, TValue, TValue> combiner, TValue defaultValue)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;

        while (enumerator.TryGetNext(out var result)) {
            if (result.IsNull)
                continue;

            var combined = result.Value;

            while (enumerator.TryGetNext(out result)) {
                if (result.HasValue) {
                    combined = combiner(combined, result.Value);
                }
            }

            return combined;
        }

        return defaultValue;
    }

    public static TValue CombineValues<TState, TEnumerator, TValue>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Func<TState, TValue, TValue, TValue> combiner, TValue defaultValue)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;

        while (enumerator.TryGetNext(out var result)) {
            if (result.IsNull)
                continue;

            var combined = result.Value;

            while (enumerator.TryGetNext(out result)) {
                if (result.HasValue) {
                    combined = combiner(state, combined, result.Value);
                }
            }

            return combined;
        }

        return defaultValue;
    }
}