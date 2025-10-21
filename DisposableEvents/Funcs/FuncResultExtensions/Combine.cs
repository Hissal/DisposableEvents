namespace DisposableEvents;

public static partial class FuncResultExtensions {
    public static FuncResult<TResult> Combine<T1, T2, TResult>(this FuncResult<T1> first, FuncResult<T2> second, Func<T1, T2, TResult> combiner) {
        if (first.HasValue && second.HasValue) {
            return FuncResult<TResult>.From(combiner(first.Value, second.Value));
        }
        return FuncResult<TResult>.Null();
    }
    
    public static FuncResult<TResult> Combine<TState, T1, T2, TResult>(this FuncResult<T1> first, FuncResult<T2> second, TState state, Func<TState, T1, T2, TResult> combiner) {
        if (first.HasValue && second.HasValue) {
            return FuncResult<TResult>.From(combiner(state, first.Value, second.Value));
        }
        return FuncResult<TResult>.Null();
    }

    public static FuncResult<TValue> Combine<TValue>(this IEnumerable<FuncResult<TValue>> results,
        Func<FuncResult<TValue>, FuncResult<TValue>, FuncResult<TValue>> combiner) 
    {
        using var enumerator = results.GetEnumerator();
        if (!enumerator.MoveNext()) {
            return FuncResult<TValue>.Null();
        }
        
        var combinedResult = enumerator.Current;
        while (enumerator.MoveNext()) {
            combinedResult = combiner(combinedResult, enumerator.Current);
        }
        return combinedResult;
    }
    
    public static FuncResult<TValue> Combine<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state,
        Func<TState, FuncResult<TValue>, FuncResult<TValue>, FuncResult<TValue>> combiner) 
    {
        using var enumerator = results.GetEnumerator();
        if (!enumerator.MoveNext()) {
            return FuncResult<TValue>.Null();
        }
        
        var combinedResult = enumerator.Current;;
        while (enumerator.MoveNext()) {
            combinedResult = combiner(state, combinedResult, enumerator.Current);
        }
        return combinedResult;
    }
    
    public static TValue? CombineValues<TValue>(this IEnumerable<FuncResult<TValue>> results,
        Func<TValue, TValue, TValue> combiner) 
    {
        using var enumerator = results.GetEnumerator();
        while (enumerator.MoveNext()) {
            if (enumerator.Current.HasValue) {
                break;
            }
        }
        
        if (!enumerator.Current.HasValue) {
            return default;
        }
        
        var combinedValue = enumerator.Current.Value;
        while (enumerator.MoveNext()) {
            if (enumerator.Current.HasValue) {
                combinedValue = combiner(combinedValue, enumerator.Current.Value);
            }
        }
        return combinedValue;
    }

    public static TValue? CombineValues<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state,
        Func<TState, TValue, TValue, TValue> combiner) 
    {
        using var enumerator = results.GetEnumerator();
        while (enumerator.MoveNext()) {
            if (enumerator.Current.HasValue) {
                break;
            }
        }
        
        if (!enumerator.Current.HasValue) {
            return default;
        }
        
        var combinedValue = enumerator.Current.Value;
        while (enumerator.MoveNext()) {
            if (enumerator.Current.HasValue) {
                combinedValue = combiner(state, combinedValue, enumerator.Current.Value);
            }
        }
        return combinedValue;
    }
    
    public static TValue CombineValues<TValue>(this IEnumerable<FuncResult<TValue>> results,
        Func<TValue, TValue, TValue> combiner, TValue defaultValue) 
    {
        using var enumerator = results.GetEnumerator();
        while (enumerator.MoveNext()) {
            if (enumerator.Current.HasValue) {
                break;
            }
        }
        
        if (!enumerator.Current.HasValue) {
            return defaultValue;
        }
        
        var combinedValue = enumerator.Current.Value;
        while (enumerator.MoveNext()) {
            if (enumerator.Current.HasValue) {
                combinedValue = combiner(combinedValue, enumerator.Current.Value);
            }
        }
        return combinedValue;
    }
    
    public static TValue CombineValues<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state,
        Func<TState, TValue, TValue, TValue> combiner, TValue defaultValue) 
    {
        using var enumerator = results.GetEnumerator();
        while (enumerator.MoveNext()) {
            if (enumerator.Current.HasValue) {
                break;
            }
        }
        
        if (!enumerator.Current.HasValue) {
            return defaultValue;
        }
        
        var combinedValue = enumerator.Current.Value;
        while (enumerator.MoveNext()) {
            if (enumerator.Current.HasValue) {
                combinedValue = combiner(state, combinedValue, enumerator.Current.Value);
            }
        }
        return combinedValue;
    }
}