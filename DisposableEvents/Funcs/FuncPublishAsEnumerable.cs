using System.Buffers;
using System.Collections;

namespace DisposableEvents;

public static class FuncPublishAsEnumerable {
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TMessage, TReturn}"/> into an
    /// <see cref="IEnumerable{T}"/> that defers work and publishes
    /// results as the sequence is enumerated (lazy/deferred execution).
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be published.</typeparam>
    /// <typeparam name="TReturn">The type of the result returned by the publisher.</typeparam>
    /// <param name="publisher">The publisher that handles the message and produces results.</param>
    /// <param name="message">The message to be published to the handlers.</param>
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
    /// <seealso cref="PublishAsEnumerableImmediate{TMessage,TReturn}(IFuncPublisher{TMessage,TReturn},TMessage)"/>
    public static IEnumerable<FuncResult<TReturn>> PublishAsEnumerable<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
        return new PublishEnumerableDeferred<TMessage, TReturn>(publisher, message);
    }
    
    /// <summary>
    /// Converts an <see cref="IFuncPublisher{TMessage, TReturn}"/> into an
    /// <see cref="IEnumerable{T}"/> that eagerly publishes to all handlers
    /// immediately and stores the results for later enumeration (eager/immediate execution).
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be published.</typeparam>
    /// <typeparam name="TReturn">The type of the result returned by the publisher.</typeparam>
    /// <param name="publisher">The publisher that handles the message and produces results.</param>
    /// <param name="message">The message to be published to the handlers.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> whose results are computed at call time
    /// and then enumerated from a buffered snapshot.
    /// </returns>
    /// <remarks>
    /// - Eager: all handlers are invoked immediately; results are buffered.<br/>
    /// - Enumeration is over the stored results; publishing does not occur during enumeration.<br/>
    /// - Useful when a stable snapshot is needed or all subscribers need to receive the message.
    /// </remarks>
    /// <seealso cref="PublishAsEnumerable{TMessage,TReturn}(IFuncPublisher{TMessage,TReturn},TMessage)"/>
    public static IEnumerable<FuncResult<TReturn>> PublishAsEnumerableImmediate<TMessage, TReturn>(this IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
        return new PublishEnumerableImmediate<TMessage, TReturn>(publisher, message);
    }
    
    sealed class PublishEnumerableDeferred<TMessage, TReturn> : IEnumerable<FuncResult<TReturn>> {
        readonly Enumerator enumerator;
        
        public PublishEnumerableDeferred(IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
            enumerator = new Enumerator(publisher, message);
        }
        
        public IEnumerator<FuncResult<TReturn>> GetEnumerator() {
            return enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        
        sealed class Enumerator : IEnumerator<FuncResult<TReturn>> {
            readonly IFuncPublisher<TMessage, TReturn> publisher;
            readonly TMessage message;
            IFuncHandler<TMessage, TReturn>[]? handlers;
            
            int index = -1;
            
            public Enumerator(IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
                this.publisher = publisher;
                this.message = message;
            }
            
            public FuncResult<TReturn> Current {
                get {
                    handlers ??= publisher.GetHandlers();
                    return publisher.PublishTo(handlers[index], message);
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
    
    sealed class PublishEnumerableImmediate<TMessage, TReturn> : IEnumerable<FuncResult<TReturn>> {
        readonly Enumerator enumerator;
        
        public PublishEnumerableImmediate(IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
            enumerator = new Enumerator(publisher, message);
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
            
            public Enumerator(IFuncPublisher<TMessage, TReturn> publisher, TMessage message) {
                var handlers = publisher.GetHandlers();
        
                if (handlers.Length == 0) {
                    results = null;
                    resultCount = 0;
                    return;
                }
        
                resultCount = handlers.Length;
                results = ArrayPool<FuncResult<TReturn>>.Shared.Rent(resultCount);
        
                for (var i = 0; i < resultCount; i++) {
                    results[i] = publisher.PublishTo(handlers[i], message);
                }
            }
            
            public FuncResult<TReturn> Current => results![index];
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
                    Console.WriteLine("Disposing PublishEnumerableImmediate and returning rented array.");
                    ArrayPool<FuncResult<TReturn>>.Shared.Return(results);
                }
            }
        }
    }
}