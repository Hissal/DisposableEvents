using ZLinq;

namespace DisposableEvents.ZLinq;

public static partial class FuncResultValueEnumerableExtensions {
    public static void Switch<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Action<TValue> onValue, Action onNull)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        while (enumerator.TryGetNext(out var result)) {
            result.Switch(onValue, onNull);
        }
    }
    
    public static void Switch<TState, TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state, Action<TState, TValue> onValue, Action<TState> onNull)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        while (enumerator.TryGetNext(out var result)) {
            result.Switch(state, onValue, onNull);
        }
    }
}