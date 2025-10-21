namespace DisposableEvents;

public static partial class FuncResultExtensions {
    public static void Switch<TValue>(this IEnumerable<FuncResult<TValue>> results, Action<TValue> onValue,
        Action onNull) {
        foreach (var result in results) {
            result.Switch(onValue, onNull);
        }
    }

    public static void Switch<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state,
        Action<TState, TValue> onValue, Action<TState> onNull) {
        foreach (var result in results) {
            result.Switch(state, onValue, onNull);
        }
    }
}