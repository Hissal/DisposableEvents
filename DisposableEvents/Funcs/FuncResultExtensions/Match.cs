namespace DisposableEvents;

public static partial class FuncResultExtensions {
    public static IEnumerable<TResult> Match<TValue, TResult>(this IEnumerable<FuncResult<TValue>> results, Func<TValue, TResult> onValue, Func<TResult> onNull) {
        foreach (var result in results) {
            yield return result.Match(onValue, onNull);
        }
    }

    public static IEnumerable<TResult> Match<TState, TValue, TResult>(this IEnumerable<FuncResult<TValue>> results, TState state, Func<TState, TValue, TResult> onValue, Func<TState, TResult> onNull) {
        foreach (var result in results) {
            yield return result.Match(state, onValue, onNull);
        }
    }

    public static IEnumerable<TResult> Match<TValue, TResult>(this IEnumerable<FuncResult<TValue>> results, Func<TValue, TResult> onValue, TResult onNull) {
        foreach (var result in results) {
            yield return result.Match(onValue, onNull);
        }
    }
    
    public static IEnumerable<TResult> Match<TState, TValue, TResult>(this IEnumerable<FuncResult<TValue>> results, TState state, Func<TState, TValue, TResult> onValue, TResult onNull) {
        foreach (var result in results) {
            yield return result.Match(state, onValue, onNull);
        }
    }
}