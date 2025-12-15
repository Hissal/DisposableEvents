using System.Buffers;
using System.Runtime.CompilerServices;
using ZLinq;
using ZLinq.Internal;

namespace DisposableEvents.ZLinq;

public static class FuncPublisherExtensionsZLinq {
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TArg, TResult}"/> into a
    /// <see cref="ValueEnumerable{TEnumerator, TItem}"/> that defers work and publishes
    /// results as the sequence is enumerated (lazy/deferred execution).
    /// </summary>
    /// <typeparam name="TArg">The type of the arg to be published.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the publisher.</typeparam>
    /// <param name="publisher">The publisher that handles the arg and produces results.</param>
    /// <param name="arg">The arg to be published to the handlers.</param>
    /// <returns>
    /// A <see cref="ValueEnumerable{TEnumerator, TItem}"/> that performs no publishing
    /// at call time; each iteration publishes to the next handler and yields its result.
    /// </returns>
    /// <remarks>
    /// - Deferred: no handlers are invoked until enumeration begins.<br/>
    /// - Publishes one-by-one while iterating; if enumeration stops early, remaining handlers are not invoked.<br/>
    /// - Minimizes upfront work and avoids buffering; useful for short-circuiting or filtering during enumeration.<br/>
    /// - Side effects happen progressively as you iterate.
    /// </remarks>
    /// <seealso cref="InvokeAsValueEnumerableImmediate{TArg,TResult}"/>
    public static ValueEnumerable<InvokeValueEnumeratorDeferred<TArg, TResult>, FuncResult<TResult>> InvokeAsValueEnumerable<TArg, TResult>(
            this IFuncPublisher<TArg, TResult> publisher,
            TArg arg) 
    {
        return new(new(publisher, arg));
    }
    
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TArg, TResult}"/> into a
    /// <see cref="ValueEnumerable{TEnumerator, TItem}"/> that eagerly publishes to all handlers
    /// immediately and stores the results for later enumeration (eager/immediate execution).
    /// </summary>
    /// <typeparam name="TArg">The type of the arg to be published.</typeparam>
    /// <typeparam name="TResult">The type of the result returned by the publisher.</typeparam>
    /// <param name="publisher">The publisher that handles the arg and produces results.</param>
    /// <param name="arg">The arg to be published to the handlers.</param>
    /// <returns>
    /// A <see cref="ValueEnumerable{TEnumerator, TItem}"/> whose results are computed at call time
    /// and then enumerated from a buffered snapshot.
    /// </returns>
    /// <remarks>
    /// - Eager: all handlers are invoked immediately; results are buffered.<br/>
    /// - Enumeration is over the stored results; publishing does not occur during enumeration.<br/>
    /// - Useful when a stable snapshot is needed or all subscribers need to receive the arg.
    /// </remarks>
    /// <seealso cref="InvokeAsValueEnumerable{TArg,TResult}"/>
    public static ValueEnumerable<InvokeValueEnumeratorImmediate<TArg, TResult>, FuncResult<TResult>> InvokeAsValueEnumerableImmediate<TArg, TResult>(
        this IFuncPublisher<TArg, TResult> publisher,
        TArg arg) 
    {
        return new(new(publisher, arg));
    }
}

public struct InvokeValueEnumeratorDeferred<TArg, TResult>(IFuncPublisher<TArg, TResult> publisher, TArg arg) : IValueEnumerator<FuncResult<TResult>> {
    IFuncHandler<TArg, TResult>[]? handlers;
    int handlerCount = 0;
    
    int index;

    IFuncHandler<TArg, TResult>[] GetHandlers() {
        if (handlers != null)
            return handlers;
                
        using var handlerSnapshot = publisher.SnapshotHandlers();
        var handlerSpan = handlerSnapshot.Span;
        
        var buffer = ArrayPool<IFuncHandler<TArg, TResult>>.Shared.Rent(handlerSpan.Length);
        handlerSpan.CopyTo(buffer.AsSpan(0, handlerSpan.Length));
        
        handlerCount = handlerSpan.Length;
        handlers = buffer;
        
        return handlers;
    }

    public bool TryGetNonEnumeratedCount(out int count) {
        handlers ??= GetHandlers();
        count = handlerCount;
        return true;
    }
    
    public bool TryGetSpan(out ReadOnlySpan<FuncResult<TResult>> span) {
        span = default;
        return false;
    }
    
    public bool TryCopyTo(Span<FuncResult<TResult>> destination, Index offset) {
        handlers ??= GetHandlers();
        
        if (EnumeratorHelper.TryGetSlice<IFuncHandler<TArg, TResult>>(
                handlers.AsSpan(0, handlerCount), 
                offset,
                destination.Length, 
                out var slice)) 
        {
            for (var i = 0; i < slice.Length; i++) {
                destination[i] = publisher.InvokeHandler(slice[i], arg);
            }
            return true;
        }
        
        return false;
    }
    
    public bool TryGetNext(out FuncResult<TResult> current) {
        handlers ??= GetHandlers();
        if (index < handlerCount) {
            current = publisher.InvokeHandler(handlers[index], arg);
            index++;
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    
    public void Dispose() {
        if (handlers == null)
            return;
                
        ArrayPool<IFuncHandler<TArg, TResult>>.Shared.Return(handlers);
        handlers = null;
    }
}

public struct InvokeValueEnumeratorImmediate<TArg, TResult> : IValueEnumerator<FuncResult<TResult>> {
    readonly FuncResult<TResult>[]? results;
    readonly int resultCount;
    
    int index;
    
    public InvokeValueEnumeratorImmediate(IFuncPublisher<TArg, TResult> publisher, TArg arg) {
        using var handlerSnapshot = publisher.SnapshotHandlers();
        var handlers = handlerSnapshot.Span;
        
        if (handlers.Length == 0) {
            results = null;
            resultCount = 0;
            index = 0;
            return;
        }
        
        resultCount = handlers.Length;
        results = ArrayPool<FuncResult<TResult>>.Shared.Rent(resultCount);
        
        for (var i = 0; i < resultCount; i++) {
            results[i] = publisher.InvokeHandler(handlers[i], arg);
        }
    }

    public bool TryGetNonEnumeratedCount(out int count) {
        count = resultCount;
        return true;
    }
    
    public bool TryGetSpan(out ReadOnlySpan<FuncResult<TResult>> span) {
        if (resultCount == 0) {
            span = default;
            return false;
        }
        
        span = results.AsSpan(0, resultCount);
        return true;
    }
    
    public bool TryCopyTo(Span<FuncResult<TResult>> destination, Index offset) {
        if (resultCount == 0) {
            return false;
        }
        
        if (EnumeratorHelper.TryGetSlice<FuncResult<TResult>>(
                results.AsSpan(0, resultCount), 
                offset,
                destination.Length, 
                out var slice)) 
        {
            slice.CopyTo(destination);
            return true;
        }
        
        return false;
    }
    
    public bool TryGetNext(out FuncResult<TResult> current) {
        if (index < resultCount) {
            current = results![index];
            index++;
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    
    public void Dispose() {
        if (results != null) {
            ArrayPool<FuncResult<TResult>>.Shared.Return(results);
        }
    }
}