using System.Runtime.CompilerServices;
using ZLinq;

namespace DisposableEvents.ZLinq;

public static partial class FuncResultValueEnumerableExtensions {
    public static ValueEnumerable<GetValuesEnumerator<TEnumerator, TValue>, FuncResult<TValue>> GetValues<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> source) where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> {
        return new(new (source.Enumerator));
    }
    
    public static ValueEnumerable<GetValuesOrDefaultNullableEnumerator<TEnumerator, TValue>, TValue?> GetValuesOrDefault<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> source) where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> {
        return new(new (source.Enumerator));
    }
    public static ValueEnumerable<GetValuesOrDefaultEnumerator<TEnumerator, TValue>, TValue> GetValuesOrDefault<TEnumerator, TValue>(this ValueEnumerable<TEnumerator, FuncResult<TValue>> source, TValue defaultValue) where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>> {
        return new(new (source.Enumerator, defaultValue));
    }
}

public struct GetValuesEnumerator<TEnumerator, TValue> : IValueEnumerator<FuncResult<TValue>>
    where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>>
{
    TEnumerator source;
    
    public GetValuesEnumerator(TEnumerator source) {
        this.source = source;
    }

    public bool TryGetNext(out FuncResult<TValue> current) {
        while (source.TryGetNext(out current)) {
            if (current.HasValue) {
                return true;
            }
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    public bool TryGetNonEnumeratedCount(out int count) {
        count = 0;
        return false;
    }
    public bool TryGetSpan(out ReadOnlySpan<FuncResult<TValue>> span) {
        span = default;
        return false;
    }
    public bool TryCopyTo(scoped Span<FuncResult<TValue>> destination, Index offset) {
        return false;
    }

    public void Dispose() {
        source.Dispose();
    }
}

public struct GetValuesOrDefaultEnumerator<TEnumerator, TValue> : IValueEnumerator<TValue>
    where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>>
{
    TEnumerator source;
    readonly TValue defaultValue;
    
    public GetValuesOrDefaultEnumerator(TEnumerator source, TValue defaultValue) {
        this.source = source;
        this.defaultValue = defaultValue;
    }

    public bool TryGetNext(out TValue current) {
        while (source.TryGetNext(out var result)) {
            current = result.HasValue ? result.Value : defaultValue;
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    
    public bool TryGetNonEnumeratedCount(out int count) {
        return source.TryGetNonEnumeratedCount(out count);
    }
    
    public bool TryGetSpan(out ReadOnlySpan<TValue> span) {
        span = default;
        return false;
    }
    
    public bool TryCopyTo(scoped Span<TValue> destination, Index offset) {
        return false;
    }

    public void Dispose() {
        source.Dispose();
    }
}

public struct GetValuesOrDefaultNullableEnumerator<TEnumerator, TValue> : IValueEnumerator<TValue?>
    where TEnumerator : struct, IValueEnumerator<FuncResult<TValue>>
{
    TEnumerator source;
    
    public GetValuesOrDefaultNullableEnumerator(TEnumerator source) {
        this.source = source;
    }

    public bool TryGetNext(out TValue? current) {
        while (source.TryGetNext(out var result)) {
            if (result.HasValue) {
                current = result.Value;
                return true;
            }
            current = default;
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    
    public bool TryGetNonEnumeratedCount(out int count) {
        return source.TryGetNonEnumeratedCount(out count);
    }
    
    public bool TryGetSpan(out ReadOnlySpan<TValue?> span) {
        span = default;
        return false;
    }
    
    public bool TryCopyTo(scoped Span<TValue?> destination, Index offset) {
        return false;
    }

    public void Dispose() {
        source.Dispose();
    }
}