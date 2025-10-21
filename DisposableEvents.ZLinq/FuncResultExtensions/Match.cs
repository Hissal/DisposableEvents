using System.Runtime.CompilerServices;
using ZLinq;

namespace DisposableEvents.ZLinq;

public static partial class FuncResultExtensions {
    public static ValueEnumerable<MatchEnumerator<TEnumerator, TValue, TResult>, TResult> Match<TEnumerator, TValue, TResult>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Func<TValue, TResult> onValue,
        Func<TResult> onNull)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        return new(new(results.Enumerator, onValue, onNull));
    }
    
    public static ValueEnumerable<MatchEnumerator<TState, TEnumerator, TValue, TResult>, TResult> Match<TState, TEnumerator, TValue, TResult>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Func<TState, TValue, TResult> onValue,
        Func<TState, TResult> onNull)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        return new(new(state, results.Enumerator, onValue, onNull));
    }
    
    public static ValueEnumerable<MatchEnumerator2<TEnumerator, TValue, TResult>, TResult> Match<TEnumerator, TValue, TResult>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        Func<TValue, TResult> onValue,
        TResult onNull)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        return new(new(results.Enumerator, onValue, onNull));
    }
    
    public static ValueEnumerable<MatchEnumerator2<TState, TEnumerator, TValue, TResult>, TResult> Match<TState, TEnumerator, TValue, TResult>(
        this ValueEnumerable<TEnumerator, FuncResult<TValue>> results,
        TState state,
        Func<TState, TValue, TResult> onValue,
        TResult onNull)
        where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> 
    {
        return new(new(state, results.Enumerator, onValue, onNull));
    }
}

public struct MatchEnumerator<TEnumerator, TValue, TResult> : IValueEnumerator<TResult>
    where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>>
{
    TEnumerator source;
    readonly Func<TValue, TResult> onValue;
    readonly Func<TResult> onNull;

    public MatchEnumerator(
        TEnumerator source,
        Func<TValue, TResult> onValue,
        Func<TResult> onNull)
    {
        this.source = source;
        this.onValue = onValue;
        this.onNull = onNull;
    }
    
    public bool TryGetNext(out TResult current) {
        while (source.TryGetNext(out var result)) {
            current = result.Match(onValue, onNull);
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    public bool TryGetNonEnumeratedCount(out int count) {
        return source.TryGetNonEnumeratedCount(out count);
    }
    public bool TryGetSpan(out ReadOnlySpan<TResult> span) {
        span = default;
        return false;
    }
    public bool TryCopyTo(Span<TResult> destination, Index offset) {
        return false;
    }
    
    public void Dispose() {
        source.Dispose();
    }
}

public struct MatchEnumerator<TState, TEnumerator, TValue, TResult> : IValueEnumerator<TResult>
    where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>>
{
    readonly TState state;
    TEnumerator source;
    readonly Func<TState, TValue, TResult> onValue;
    readonly Func<TState, TResult> onNull;

    public MatchEnumerator(
        TState state,
        TEnumerator source,
        Func<TState, TValue, TResult> onValue,
        Func<TState, TResult> onNull)
    {
        this.state = state;
        this.source = source;
        this.onValue = onValue;
        this.onNull = onNull;
    }
    
    public bool TryGetNext(out TResult current) {
        while (source.TryGetNext(out var result)) {
            current = result.Match(state, onValue, onNull);
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    public bool TryGetNonEnumeratedCount(out int count) {
        return source.TryGetNonEnumeratedCount(out count);
    }
    public bool TryGetSpan(out ReadOnlySpan<TResult> span) {
        span = default;
        return false;
    }
    public bool TryCopyTo(Span<TResult> destination, Index offset) {
        return false;
    }
    
    public void Dispose() {
        source.Dispose();
    }
}

public struct MatchEnumerator2<TEnumerator, TValue, TResult> : IValueEnumerator<TResult>
    where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>>
{
    TEnumerator source;
    readonly Func<TValue, TResult> onValue;
    readonly TResult onNull;

    public MatchEnumerator2(
        TEnumerator source,
        Func<TValue, TResult> onValue,
        TResult onNull)
    {
        this.source = source;
        this.onValue = onValue;
        this.onNull = onNull;
    }
    
    public bool TryGetNext(out TResult current) {
        while (source.TryGetNext(out var result)) {
            current = result.Match(onValue, onNull);
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    public bool TryGetNonEnumeratedCount(out int count) {
        return source.TryGetNonEnumeratedCount(out count);
    }
    public bool TryGetSpan(out ReadOnlySpan<TResult> span) {
        span = default;
        return false;
    }
    public bool TryCopyTo(Span<TResult> destination, Index offset) {
        return false;
    }
    
    public void Dispose() {
        source.Dispose();
    }
}

public struct MatchEnumerator2<TState, TEnumerator, TValue, TResult> : IValueEnumerator<TResult>
    where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>>
{
    readonly TState state;
    TEnumerator source;
    readonly Func<TState, TValue, TResult> onValue;
    readonly TResult onNull;

    public MatchEnumerator2(
        TState state,
        TEnumerator source,
        Func<TState, TValue, TResult> onValue,
        TResult onNull)
    {
        this.state = state;
        this.source = source;
        this.onValue = onValue;
        this.onNull = onNull;
    }
    
    public bool TryGetNext(out TResult current) {
        while (source.TryGetNext(out var result)) {
            current = result.Match(state, onValue, onNull);
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    public bool TryGetNonEnumeratedCount(out int count) {
        return source.TryGetNonEnumeratedCount(out count);
    }
    public bool TryGetSpan(out ReadOnlySpan<TResult> span) {
        span = default;
        return false;
    }
    public bool TryCopyTo(Span<TResult> destination, Index offset) {
        return false;
    }
    
    public void Dispose() {
        source.Dispose();
    }
}