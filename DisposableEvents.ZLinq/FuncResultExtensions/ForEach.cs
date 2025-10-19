using ZLinq;

namespace DisposableEvents.ZLinq;

public static partial class FuncResultValueEnumerableExtensions {
    public static void ForEach<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Action<FuncResult<TValue>> forEach)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        while (enumerator.TryGetNext(out var result)) {
            forEach(result);
        }
    }
    
    public static void ForEach<TState, TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Action<TState, FuncResult<TValue>> forEach)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        while (enumerator.TryGetNext(out var result)) {
            forEach(state, result);
        }
    }
    
    public static void ForEach<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Action<FuncResult<TValue>, int> forEach)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        var index = 0;
        while (enumerator.TryGetNext(out var result)) {
            forEach(result, index);
            index++;
        }
    }
    
    public static void ForEach<TState, TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Action<TState, FuncResult<TValue>, int> forEach)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        var index = 0;
        while (enumerator.TryGetNext(out var result)) {
            forEach(state, result, index);
            index++;
        }
    }
    
    public static void ForEachValue<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Action<TValue> forEach)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        while (enumerator.TryGetNext(out var result)) {
            if (result.HasValue) {
                forEach(result.Value);
            }
        }
    }
    
    public static void ForEachValue<TState, TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Action<TState, TValue> forEach)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        while (enumerator.TryGetNext(out var result)) {
            if (result.HasValue) {
                forEach(state, result.Value);
            }
        }
    }
    
    public static void ForEachValue<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Action<TValue, int> forEach)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        var index = 0;
        while (enumerator.TryGetNext(out var result)) {
            if (result.HasValue) {
                forEach(result.Value, index);
            }
            index++;
        }
    }
    
    public static void ForEachValue<TState, TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Action<TState, TValue, int> forEach)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        using var enumerator = results.Enumerator;
        var index = 0;
        while (enumerator.TryGetNext(out var result)) {
            if (result.HasValue) {
                forEach(state, result.Value, index);
            }
            index++;
        }
    }
}