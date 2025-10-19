namespace DisposableEvents;

public static class FuncResultExtensions {
    public static FuncResult<TResult> Combine<T1, T2, TResult>(this FuncResult<T1> first, FuncResult<T2> second, Func<T1, T2, TResult> combiner) {
        if (first.HasValue && second.HasValue) {
            return FuncResult<TResult>.From(combiner(first.Value, second.Value));
        }
        return FuncResult<TResult>.Null();
    }
    
    public static int CountValues<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        return results.Count(result => result.HasValue);
    }
    
    public static bool AnyValue<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        return results.Any(result => result.HasValue);
    }
    
    public static bool AllValues<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        return results.All(result => result.HasValue);
    }
    
    public static int CountNulls<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        return results.Count(result => result.IsNull);
    }
    
    public static bool AnyNull<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        return results.Any(result => result.IsNull);
    }
    
    public static bool AllNulls<TValue>(this IEnumerable<FuncResult<TValue>> results) {
        return results.All(result => result.IsNull);
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
    
    public static void ForEachNull<TValue>(this IEnumerable<FuncResult<TValue>> results, Action forEach) {
        foreach (var result in results) {
            if (!result.HasValue) {
                forEach();
            }
        }
    }
    
    public static void ForEachNull<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Action<TState> forEach) {
        foreach (var result in results) {
            if (!result.HasValue) {
                forEach(state);
            }
        }
    }
    
    public static IEnumerable<FuncResult<TValue>> Switch<TValue>(this IEnumerable<FuncResult<TValue>> results, Action<TValue> onValue, Action onNull) {
        foreach (var result in results) {
            result.Switch(onValue, onNull);
            yield return result;
        }
    }
    
    public static IEnumerable<FuncResult<TValue>> Switch<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Action<TState, TValue> onValue, Action<TState> onNull) {
        foreach (var result in results) {
            result.Switch(state, onValue, onNull);
            yield return result;
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

    public static IEnumerable<FuncResult<TResult>> Select<TValue, TResult>(this IEnumerable<FuncResult<TValue>> results, Func<TValue, TResult> selector) {
        foreach (var result in results) {
            yield return result.Select(selector);
        }
    }
    
    public static IEnumerable<FuncResult<TResult>> Select<TState, TValue, TResult>(this IEnumerable<FuncResult<TValue>> results, TState state, Func<TState, TValue, TResult> selector) {
        foreach (var result in results) {
            yield return result.Select(state, selector);
        }
    }
    
    public static IEnumerable<FuncResult<TResult>> SelectMany<TValue, TResult>(this IEnumerable<FuncResult<TValue>> results, Func<TValue, FuncResult<TResult>> selector) {
        foreach (var result in results) {
            yield return result.SelectMany(selector);
        }
    }
    
    public static IEnumerable<FuncResult<TResult>> SelectMany<TState, TValue, TResult>(this IEnumerable<FuncResult<TValue>> results, TState state, Func<TState, TValue, FuncResult<TResult>> selector) {
        foreach (var result in results) {
            yield return result.SelectMany(state, selector);
        }
    }
    
    public static IEnumerable<FuncResult<TValue>> Where<TValue>(this IEnumerable<FuncResult<TValue>> results, Func<TValue, bool> predicate) {
        foreach (var result in results) {
            var filtered = result.Where(predicate);
            if (filtered.HasValue)
                yield return filtered;
        }
    }
    
    public static IEnumerable<FuncResult<TValue>> Where<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Func<TState, TValue, bool> predicate) {
        foreach (var result in results) {
            var filtered = result.Where(state, predicate);
            if (filtered.HasValue)
                yield return filtered;
        }
    }
    
    public static IEnumerable<FuncResult<TValue>> Where<TValue>(this IEnumerable<FuncResult<TValue>> results, Func<FuncResult<TValue>, bool> predicate) {
        foreach (var result in results) {
            var filtered = result.Where(predicate);
            if (filtered.HasValue)
                yield return filtered;
        }
    }
    
    public static IEnumerable<FuncResult<TValue>> Where<TState, TValue>(this IEnumerable<FuncResult<TValue>> results, TState state, Func<TState, FuncResult<TValue>, bool> predicate) {
        foreach (var result in results) {
            var filtered = result.Where(state, predicate);
            if (filtered.HasValue)
                yield return filtered;
        }
    }
}