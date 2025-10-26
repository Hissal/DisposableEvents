﻿using System.Buffers;
using System.Runtime.CompilerServices;
using ZLinq;
using ZLinq.Internal;

namespace DisposableEvents.ZLinq;

public static class FuncPublisherExtensions {
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TArg, TReturn}"/> into a
    /// <see cref="ValueEnumerable{TEnumerator, TItem}"/> that defers work and publishes
    /// results as the sequence is enumerated (lazy/deferred execution).
    /// </summary>
    /// <typeparam name="TArg">The type of the arg to be published.</typeparam>
    /// <typeparam name="TReturn">The type of the result returned by the publisher.</typeparam>
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
    /// <seealso cref="InvokeAsValueEnumerableImmediate{TArg,TReturn}"/>
    public static ValueEnumerable<InvokeValueEnumeratorDeferred<TArg, TReturn>, FuncResult<TReturn>> InvokeAsValueEnumerable<TArg, TReturn>(
            this IFuncPublisher<TArg, TReturn> publisher,
            TArg arg) 
    {
        return new(new(publisher, arg));
    }
    
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TArg, TReturn}"/> into a
    /// <see cref="ValueEnumerable{TEnumerator, TItem}"/> that eagerly publishes to all handlers
    /// immediately and stores the results for later enumeration (eager/immediate execution).
    /// </summary>
    /// <typeparam name="TArg">The type of the arg to be published.</typeparam>
    /// <typeparam name="TReturn">The type of the result returned by the publisher.</typeparam>
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
    /// <seealso cref="InvokeAsValueEnumerable{TArg,TReturn}"/>
    public static ValueEnumerable<InvokeValueEnumeratorImmediate<TArg, TReturn>, FuncResult<TReturn>> InvokeAsValueEnumerableImmediate<TArg, TReturn>(
        this IFuncPublisher<TArg, TReturn> publisher,
        TArg arg) 
    {
        return new(new(publisher, arg));
    }
}

public struct InvokeValueEnumeratorDeferred<TArg, TReturn> : IValueEnumerator<FuncResult<TReturn>> {
    readonly IFuncPublisher<TArg, TReturn> publisher;
    readonly TArg arg;
    IFuncHandler<TArg, TReturn>[]? handlers;
    
    int index;
    
    public InvokeValueEnumeratorDeferred(IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
        this.publisher = publisher;
        this.arg = arg;
    }

    public bool TryGetNonEnumeratedCount(out int count) {
        count = (handlers ??= publisher.GetHandlers()).Length;
        return true;
    }
    
    public bool TryGetSpan(out ReadOnlySpan<FuncResult<TReturn>> span) {
        span = default;
        return false;
    }
    
    public bool TryCopyTo(Span<FuncResult<TReturn>> destination, Index offset) {
        handlers ??= publisher.GetHandlers();
        
        if (EnumeratorHelper.TryGetSlice<IFuncHandler<TArg, TReturn>>(
                handlers.AsSpan(), 
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
    
    public bool TryGetNext(out FuncResult<TReturn> current) {
        handlers ??= publisher.GetHandlers();
        if (index < handlers.Length) {
            current = publisher.InvokeHandler(handlers[index], arg);
            index++;
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    
    public void Dispose() {
        
    }
}

public struct InvokeValueEnumeratorImmediate<TArg, TReturn> : IValueEnumerator<FuncResult<TReturn>> {
    readonly FuncResult<TReturn>[]? results;
    readonly int resultCount;
    
    int index;
    
    public InvokeValueEnumeratorImmediate(IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
        var handlers = publisher.GetHandlers();
        
        if (handlers.Length == 0) {
            results = null;
            resultCount = 0;
            index = 0;
            return;
        }
        
        resultCount = handlers.Length;
        results = ArrayPool<FuncResult<TReturn>>.Shared.Rent(resultCount);
        
        for (var i = 0; i < resultCount; i++) {
            results[i] = publisher.InvokeHandler(handlers[i], arg);
        }
    }

    public bool TryGetNonEnumeratedCount(out int count) {
        count = resultCount;
        return true;
    }
    
    public bool TryGetSpan(out ReadOnlySpan<FuncResult<TReturn>> span) {
        if (resultCount == 0) {
            span = default;
            return false;
        }
        
        span = results.AsSpan(0, resultCount);
        return true;
    }
    
    public bool TryCopyTo(Span<FuncResult<TReturn>> destination, Index offset) {
        if (resultCount == 0) {
            return false;
        }
        
        if (EnumeratorHelper.TryGetSlice<FuncResult<TReturn>>(
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
    
    public bool TryGetNext(out FuncResult<TReturn> current) {
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
            ArrayPool<FuncResult<TReturn>>.Shared.Return(results);
        }
    }
}