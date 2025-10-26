using System.Buffers;
using System.Collections;

namespace DisposableEvents;

public static partial class FuncPublisherExtensions {
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TArg, TReturn}"/> into an
    /// <see cref="IEnumerable{T}"/> that defers work and publishes
    /// results as the sequence is enumerated (lazy/deferred execution).
    /// </summary>
    /// <typeparam name="TArg">The type of the arg to be published.</typeparam>
    /// <typeparam name="TReturn">The type of the result returned by the publisher.</typeparam>
    /// <param name="publisher">The publisher that handles the arg and produces results.</param>
    /// <param name="arg">The arg to be published to the handlers.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> that performs no publishing at call time; each iteration
    /// publishes to the next handler and yields its result.
    /// </returns>
    /// <remarks>
    /// - Deferred: no handlers are invoked until enumeration begins.<br/>
    /// - Publishes one-by-one while iterating; if enumeration stops early, remaining handlers are not invoked.<br/>
    /// - Minimizes upfront work and avoids buffering; useful for short-circuiting or filtering during enumeration.<br/>
    /// - Side effects happen progressively as you iterate.
    /// </remarks>
    /// <seealso cref="InvokeAsEnumerableImmediate{TArg,TReturn}"/>
    public static IEnumerable<FuncResult<TReturn>> InvokeAsEnumerable<TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
        return new PublishEnumerableDeferred<TArg, TReturn>(publisher, arg);
    }
    
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TArg, TReturn}"/> into an
    /// <see cref="IEnumerable{T}"/> that eagerly publishes to all handlers
    /// immediately and stores the results for later enumeration (eager/immediate execution).
    /// </summary>
    /// <typeparam name="TArg">The type of the arg to be published.</typeparam>
    /// <typeparam name="TReturn">The type of the result returned by the publisher.</typeparam>
    /// <param name="publisher">The publisher that handles the arg and produces results.</param>
    /// <param name="arg">The arg to be published to the handlers.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> whose results are computed at call time
    /// and then enumerated from a buffered snapshot.
    /// </returns>
    /// <remarks>
    /// - Eager: all handlers are invoked immediately; results are buffered.<br/>
    /// - Enumeration is over the stored results; publishing does not occur during enumeration.<br/>
    /// - Useful when a stable snapshot is needed or all subscribers need to receive the arg.
    /// </remarks>
    /// <seealso cref="InvokeAsEnumerable{TArg,TReturn}"/>
    public static IEnumerable<FuncResult<TReturn>> InvokeAsEnumerableImmediate<TArg, TReturn>(this IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
        return new PublishEnumerableImmediate<TArg, TReturn>(publisher, arg);
    }
    
    // ----- Classes ----- //
    
    sealed class PublishEnumerableDeferred<TArg, TReturn> : IEnumerable<FuncResult<TReturn>> {
        readonly Enumerator enumerator;
        
        public PublishEnumerableDeferred(IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
            enumerator = new Enumerator(publisher, arg);
        }
        
        public IEnumerator<FuncResult<TReturn>> GetEnumerator() {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        sealed class Enumerator : IEnumerator<FuncResult<TReturn>> {
            readonly IFuncPublisher<TArg, TReturn> publisher;
            readonly TArg arg;
            IFuncHandler<TArg, TReturn>[]? handlers;
            
            int index = -1;
            
            public Enumerator(IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
                this.publisher = publisher;
                this.arg = arg;
            }
            
            public FuncResult<TReturn> Current {
                get {
                    handlers ??= publisher.GetHandlers();
                    return publisher.InvokeHandler(handlers[index], arg);
                }
            }

            object IEnumerator.Current => Current;
            
            public bool MoveNext() {
                handlers ??= publisher.GetHandlers();
                index++;
                return index < handlers.Length;
            }
            
            public void Reset() {
                handlers = null;
                index = -1;
            }

            public void Dispose() {
            }
        }
    }
    
    sealed class PublishEnumerableImmediate<TArg, TReturn> : IEnumerable<FuncResult<TReturn>> {
        readonly Enumerator enumerator;
        
        public PublishEnumerableImmediate(IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
            enumerator = new Enumerator(publisher, arg);
        }
        
        public IEnumerator<FuncResult<TReturn>> GetEnumerator() {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        sealed class Enumerator : IEnumerator<FuncResult<TReturn>> {
            readonly FuncResult<TReturn>[]? results;
            readonly int resultCount;
            
            int index = -1;
            
            public Enumerator(IFuncPublisher<TArg, TReturn> publisher, TArg arg) {
                var handlers = publisher.GetHandlers();
        
                if (handlers.Length == 0) {
                    results = null;
                    resultCount = 0;
                    return;
                }
        
                resultCount = handlers.Length;
                results = ArrayPool<FuncResult<TReturn>>.Shared.Rent(resultCount);
        
                for (var i = 0; i < resultCount; i++) {
                    results[i] = publisher.InvokeHandler(handlers[i], arg);
                }
            }
            
            public FuncResult<TReturn> Current => results?[index] ?? FuncResult<TReturn>.Null();
            object IEnumerator.Current => Current;
            
            public bool MoveNext() {
                if (results == null)
                    return false;
                
                index++;
                return index < resultCount;
            }
            
            public void Reset() {
                index = -1;
            }

            public void Dispose() {
                if (results != null) {
                    ArrayPool<FuncResult<TReturn>>.Shared.Return(results);
                }
            }
        }
    }
}