namespace DisposableEvents;

public static partial class FuncResultExtensions {
    public static void ForEach<TValue>(this IEnumerable<FuncResult<TValue>> results,
        Action<FuncResult<TValue>> forEach) {
        foreach (var result in results) {
            forEach(result);
        }
    }

    public static void ForEach<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state,
        Action<TState, FuncResult<TValue>> forEach) {
        foreach (var result in results) {
            forEach(state, result);
        }
    }

    public static void ForEach<TValue>(this IEnumerable<FuncResult<TValue>> results,
        Action<FuncResult<TValue>, int> forEach) {
        var currentIndex = 0;
        foreach (var result in results) {
            forEach(result, currentIndex);
            currentIndex++;
        }
    }

    public static void ForEach<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state,
        Action<TState, FuncResult<TValue>, int> forEach) {
        var currentIndex = 0;
        foreach (var result in results) {
            forEach(state, result, currentIndex);
            currentIndex++;
        }
    }

    public static void ForEachValue<TValue>(this IEnumerable<FuncResult<TValue>> results, Action<TValue> forEach) {
        foreach (var result in results) {
            if (result.HasValue) {
                forEach(result.Value);
            }
        }
    }

    public static void ForEachValue<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state,
        Action<TState, TValue> forEach) {
        foreach (var result in results) {
            if (result.HasValue) {
                forEach(state, result.Value);
            }
        }
    }

    public static void ForEachValue<TValue>(this IEnumerable<FuncResult<TValue>> results, Action<TValue, int> forEach) {
        var currentIndex = 0;
        foreach (var result in results) {
            if (result.HasValue) {
                forEach(result.Value, currentIndex);
            }

            currentIndex++;
        }
    }

    public static void ForEachValue<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state,
        Action<TState, TValue, int> forEach) {
        var currentIndex = 0;
        foreach (var result in results) {
            if (result.HasValue) {
                forEach(state, result.Value, currentIndex);
            }

            currentIndex++;
        }
    }
}