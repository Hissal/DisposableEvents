namespace DisposableEvents;

public static class FuncResultExtensions {
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
        
        var combinedResult = enumerator.Current;;
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
    
    public static IEnumerable<TValue> GetValuesOrDefault<TValue>(this IEnumerable<FuncResult<TValue>> results, TValue defaultValue) {
        foreach (var result in results) {
            yield return result.HasValue ? result.Value : defaultValue;
        }
    }
    
    public static void ForEach<TValue>(this IEnumerable<FuncResult<TValue>> results, Action<FuncResult<TValue>> forEach) {
        foreach (var result in results) {
            forEach(result);
        }
    }
    
    public static void ForEach<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Action<TState, FuncResult<TValue>> forEach) {
        foreach (var result in results) {
            forEach(state, result);
        }
    }
    
    public static void ForEach<TValue>(this IEnumerable<FuncResult<TValue>> results, Action<FuncResult<TValue>, int> forEach) {
        var currentIndex = 0;
        foreach (var result in results) {
            forEach(result, currentIndex);
            currentIndex++;
        }
    }
    
    public static void ForEach<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Action<TState, FuncResult<TValue>, int> forEach) {
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
    
    public static void ForEachValue<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Action<TState, TValue> forEach) {
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
    
    public static void ForEachValue<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Action<TState, TValue, int> forEach) {
        var currentIndex = 0;
        foreach (var result in results) {
            if (result.HasValue) {
                forEach(state, result.Value, currentIndex);
            }
            currentIndex++;
        }
    }
    
    public static void Switch<TValue>(this IEnumerable<FuncResult<TValue>> results, Action<TValue> onValue, Action onNull) {
        foreach (var result in results) {
            result.Switch(onValue, onNull);
        }
    }
    
    public static void Switch<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Action<TState, TValue> onValue, Action<TState> onNull) {
        foreach (var result in results) {
            result.Switch(state, onValue, onNull);
        }
    }

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