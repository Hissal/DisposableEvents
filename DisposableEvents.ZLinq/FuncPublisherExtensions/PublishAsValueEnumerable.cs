using System.Buffers;
using System.Runtime.CompilerServices;
using ZLinq;
using ZLinq.Internal;

namespace DisposableEvents.ZLinq;

public static class FuncPublisherExtensions {
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TMessage, TReturn}"/> into a
    /// <see cref="ValueEnumerable{TEnumerator, TItem}"/> that defers work and publishes
    /// results as the sequence is enumerated (lazy/deferred execution).
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be published.</typeparam>
    /// <typeparam name="TReturn">The type of the result returned by the publisher.</typeparam>
    /// <param name="publisher">The publisher that handles the message and produces results.</param>
    /// <param name="message">The message to be published to the handlers.</param>
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
    /// <seealso cref="PublishAsValueEnumerableImmediate{TMessage,TReturn}(IFuncPublisher{TMessage,TReturn},TMessage)"/>
    public static ValueEnumerable<PublishValueEnumeratorDeferred<TMessage, TReturn>, FuncResult<TReturn>> PublishAsValueEnumerable<TMessage, TReturn>(
            this IFuncPublisher<TMessage, TReturn> publisher,
            TMessage message) 
    {
        return new(new(publisher, message));
    }
    
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TMessage, TReturn}"/> into a
    /// <see cref="ValueEnumerable{TEnumerator, TItem}"/> that eagerly publishes to all handlers
    /// immediately and stores the results for later enumeration (eager/immediate execution).
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be published.</typeparam>
    /// <typeparam name="TReturn">The type of the result returned by the publisher.</typeparam>
    /// <param name="publisher">The publisher that handles the message and produces results.</param>
    /// <param name="message">The message to be published to the handlers.</param>
    /// <returns>
    /// A <see cref="ValueEnumerable{TEnumerator, TItem}"/> whose results are computed at call time
    /// and then enumerated from a buffered snapshot.
    /// </returns>
    /// <remarks>
    /// - Eager: all handlers are invoked immediately; results are buffered.<br/>
    /// - Enumeration is over the stored results; publishing does not occur during enumeration.<br/>
    /// - Useful when a stable snapshot is needed or all subscribers need to receive the message.
    /// </remarks>
    /// <seealso cref="PublishAsValueEnumerable{TMessage,TReturn}(IFuncPublisher{TMessage,TReturn},TMessage)"/>
    public static ValueEnumerable<PublishValueEnumeratorImmediate<TMessage, TReturn>, FuncResult<TReturn>> PublishAsValueEnumerableImmediate<TMessage, TReturn>(
        this IFuncPublisher<TMessage, TReturn> publisher,
        TMessage message) 
    {
        return new(new(publisher, message));
    }
}

public struct PublishValueEnumeratorDeferred<TMessage, TReturn> : IValueEnumerator<FuncResult<TReturn>> {
    readonly IFuncPublisher<TMessage, TReturn> publisher;
    readonly TMessage message;
    IFuncHandler<TMessage, TReturn>[]? handlers;
    
    int index;
    
    public PublishValueEnumeratorDeferred(IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
        this.publisher = publisher;
        this.message = message;
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
        
        if (EnumeratorHelper.TryGetSlice<IFuncHandler<TMessage, TReturn>>(
                handlers.AsSpan(), 
                offset,
                destination.Length, 
                out var slice)) 
        {
            for (var i = 0; i < slice.Length; i++) {
                destination[i] = publisher.PublishTo(slice[i], message);
            }
            return true;
        }
        
        return false;
    }
    
    public bool TryGetNext(out FuncResult<TReturn> current) {
        handlers ??= publisher.GetHandlers();
        if (index < handlers.Length) {
            current = publisher.PublishTo(handlers[index], message);
            index++;
            return true;
        }
        
        Unsafe.SkipInit(out current);
        return false;
    }
    
    public void Dispose() {
        
    }
}

public struct PublishValueEnumeratorImmediate<TMessage, TReturn> : IValueEnumerator<FuncResult<TReturn>> {
    readonly FuncResult<TReturn>[]? results;
    readonly int resultCount;
    
    int index;
    
    public PublishValueEnumeratorImmediate(IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
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
            results[i] = publisher.PublishTo(handlers[i], message);
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