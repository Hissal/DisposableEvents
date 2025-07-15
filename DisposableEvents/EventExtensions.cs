using System.Buffers;
using DisposableEvents.Factories;

namespace DisposableEvents;

/// <summary>
/// Provides extension methods for subscribing to and publishing events with various observer and filter configurations.
/// </summary>
public static partial class EventExtensions {
    // Normal event subscription methods

    /// <summary>
    /// Subscribes to the event with an action and optional filters.
    /// </summary>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        Action<TMessage> action,
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.Subscribe(new EventObserver<TMessage>(action), filters);
    }

    /// <summary>
    /// Subscribes to the event with an action, error handler, completion handler, and optional filters.
    /// </summary>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onComplete">The action to invoke on completion.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        Action<TMessage> action,
        Action<Exception>? onError = null, 
        Action? onComplete = null, 
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.Subscribe(new EventObserver<TMessage>(action, onError, onComplete), filters);
    }

    /// <summary>
    /// Subscribes to the event with an action and a predicate filter.
    /// </summary>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="eventFilter">Predicate filter for event messages.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        Action<TMessage> action,
        Func<TMessage, bool> eventFilter) 
    {
        return subscriber.Subscribe(
            new EventObserver<TMessage>(action),
            new PredicateEventFilter<TMessage>(eventFilter)
        );
    }

    /// <summary>
    /// Subscribes to the event with an action and predicate filters for event, error, and completion.
    /// </summary>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onComplete">The action to invoke on completion.</param>
    /// <param name="eventFilter">Predicate filter for event messages.</param>
    /// <param name="errorFilter">Predicate filter for errors.</param>
    /// <param name="completedFilter">Predicate filter for completion.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        Action<TMessage> action,
        Action<Exception>? onError = null, 
        Action? onComplete = null, 
        Func<TMessage, bool>? eventFilter = null,
        Func<Exception, bool>? errorFilter = null, 
        Func<bool>? completedFilter = null) 
    {
        return subscriber.Subscribe(
            new EventObserver<TMessage>(action, onError, onComplete),
            new PredicateEventFilter<TMessage>(eventFilter, errorFilter, completedFilter)
        );
    }
    
    /// <summary>
    /// Subscribes to the event with an observer and optional predicate filters for event, error, and completion.
    /// </summary>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="observer">The observer to receive event notifications.</param>
    /// <param name="eventFilter">Predicate filter for event messages. If null, all events are allowed.</param>
    /// <param name="errorFilter">Predicate filter for errors. If null, all errors are allowed.</param>
    /// <param name="completedFilter">Predicate filter for completion. If null, all completions are allowed.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TMessage>(
        this IEventSubscriber<TMessage> subscriber, 
        IObserver<TMessage> observer,
        Func<TMessage, bool>? eventFilter = null,
        Func<Exception, bool>? errorFilter = null, 
        Func<bool>? completedFilter = null) 
    {
        return subscriber.Subscribe(observer, new PredicateEventFilter<TMessage>(eventFilter, errorFilter, completedFilter));
    }

    // Closure capturing event subscription methods

    /// <summary>
    /// Subscribes to the event with a closure and action, with optional filters.
    /// </summary>
    /// <typeparam name="TClosure">The closure type.</typeparam>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="closure">The closure value.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TClosure, TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        TClosure closure, 
        Action<TClosure, TMessage> action, 
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.Subscribe(new ClosureEventObserver<TClosure, TMessage>(closure, action), filters);
    }

    /// <summary>
    /// Subscribes to the event with a closure, action, error handler, completion handler, and optional filters.
    /// </summary>
    /// <typeparam name="TClosure">The closure type.</typeparam>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="closure">The closure value.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onComplete">The action to invoke on completion.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TClosure, TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        TClosure closure, 
        Action<TClosure, TMessage> action, 
        Action<TClosure, Exception>? onError = null,
        Action<TClosure>? onComplete = null, 
        params IEventFilter<TMessage>[] filters) 
    {
        return subscriber.Subscribe(new ClosureEventObserver<TClosure, TMessage>(closure, action, onError, onComplete), filters);
    }

    /// <summary>
    /// Subscribes to the event with a closure, action, and a predicate filter.
    /// </summary>
    /// <typeparam name="TClosure">The closure type.</typeparam>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="closure">The closure value.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="eventFilter">Predicate filter for event messages.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TClosure, TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        TClosure closure, 
        Action<TClosure, TMessage> action, 
        Func<TMessage, bool> eventFilter)
    {
        return subscriber.Subscribe(
            new ClosureEventObserver<TClosure, TMessage>(closure, action),
            new PredicateEventFilter<TMessage>(eventFilter)
        );
    }

    /// <summary>
    /// Subscribes to the event with a closure, action, and predicate filters for event, error, and completion.
    /// </summary>
    /// <typeparam name="TClosure">The closure type.</typeparam>
    /// <typeparam name="TMessage">The event message type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="closure">The closure value.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onComplete">The action to invoke on completion.</param>
    /// <param name="eventFilter">Predicate filter for event messages.</param>
    /// <param name="errorFilter">Predicate filter for errors.</param>
    /// <param name="completedFilter">Predicate filter for completion.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TClosure, TMessage>(
        this IEventSubscriber<TMessage> subscriber,
        TClosure closure, 
        Action<TClosure, TMessage> action, 
        Action<TClosure, Exception>? onError = null,
        Action<TClosure>? onComplete = null, 
        Func<TMessage, bool>? eventFilter = null,
        Func<Exception, bool>? errorFilter = null, 
        Func<bool>? completedFilter = null) 
    {
        return subscriber.Subscribe(
            new ClosureEventObserver<TClosure, TMessage>(closure, action, onError, onComplete),
            new PredicateEventFilter<TMessage>(eventFilter, errorFilter, completedFilter)
        );
    }

    // Empty event methods

    /// <summary>
    /// Subscribes to an empty event with an action and optional filters.
    /// </summary>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe(
        this IEventSubscriber<EmptyEvent> subscriber, 
        Action action,
        params IEventFilter<EmptyEvent>[] filters) 
    {
        return subscriber.Subscribe(new EmptyEventObserver(action), filters);
    }

    /// <summary>
    /// Subscribes to an empty event with an action, error handler, completion handler, and optional filters.
    /// </summary>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onComplete">The action to invoke on completion.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe(
        this IEventSubscriber<EmptyEvent> subscriber, 
        Action action,
        Action<Exception>? onError = null, 
        Action? onComplete = null, 
        params IEventFilter<EmptyEvent>[] filters) 
    {
        return subscriber.Subscribe(new EmptyEventObserver(action, onError, onComplete), filters);
    }

    /// <summary>
    /// Subscribes to an empty event with an action and a predicate filter.
    /// </summary>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="eventFilter">Predicate filter for event messages.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe(
        this IEventSubscriber<EmptyEvent> subscriber, 
        Action action,
        Func<EmptyEvent, bool> eventFilter) 
    {
        return subscriber.Subscribe(
            new EmptyEventObserver(action), 
            new PredicateEventFilter<EmptyEvent>(eventFilter)
        );
    }

    /// <summary>
    /// Subscribes to an empty event with an action and predicate filters for event, error, and completion.
    /// </summary>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onComplete">The action to invoke on completion.</param>
    /// <param name="eventFilter">Predicate filter for event messages.</param>
    /// <param name="errorFilter">Predicate filter for errors.</param>
    /// <param name="completedFilter">Predicate filter for completion.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe(
        this IEventSubscriber<EmptyEvent> subscriber, 
        Action action,
        Action<Exception>? onError = null, 
        Action? onComplete = null, 
        Func<EmptyEvent, bool>? eventFilter = null,
        Func<Exception, bool>? errorFilter = null, 
        Func<bool>? completedFilter = null) 
    {
        return subscriber.Subscribe(
            new EmptyEventObserver(action, onError, onComplete),
            new PredicateEventFilter<EmptyEvent>(eventFilter, errorFilter, completedFilter)
        );
    }
    
    /// <summary>
    /// Subscribes to an empty event with an observer and optional predicate filters for event, error, and completion.
    /// </summary>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="observer">The observer to receive event notifications.</param>
    /// <param name="eventFilter">Predicate filter for event messages. If null, all events are allowed.</param>
    /// <param name="errorFilter">Predicate filter for errors. If null, all errors are allowed.</param>
    /// <param name="completedFilter">Predicate filter for completion. If null, all completions are allowed.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe(
        this IEventSubscriber<EmptyEvent> subscriber, 
        IObserver<EmptyEvent> observer,
        Func<bool>? eventFilter = null,
        Func<Exception, bool>? errorFilter = null, 
        Func<bool>? completedFilter = null) 
    {
        return subscriber.Subscribe(observer, new EmptyPredicateEventFilter(eventFilter, errorFilter, completedFilter));
    }

    // Empty event closure capturing methods

    /// <summary>
    /// Subscribes to an empty event with a closure and action, with optional filters.
    /// </summary>
    /// <typeparam name="TClosure">The closure type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="closure">The closure value.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TClosure>(
        this IEventSubscriber<EmptyEvent> subscriber, 
        TClosure closure,
        Action<TClosure> action, 
        params IEventFilter<EmptyEvent>[] filters) 
    {
        return subscriber.Subscribe(new EmptyClosureEventObserver<TClosure>(closure, action), filters);
    }

    /// <summary>
    /// Subscribes to an empty event with a closure, action, error handler, completion handler, and optional filters.
    /// </summary>
    /// <typeparam name="TClosure">The closure type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="closure">The closure value.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onComplete">The action to invoke on completion.</param>
    /// <param name="filters">Optional event filters.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TClosure>(
        this IEventSubscriber<EmptyEvent> subscriber, 
        TClosure closure,
        Action<TClosure> action, 
        Action<TClosure, Exception>? onError = null, 
        Action<TClosure>? onComplete = null,
        params IEventFilter<EmptyEvent>[] filters) 
    {
        return subscriber.Subscribe(new EmptyClosureEventObserver<TClosure>(closure, action, onError, onComplete), filters);
    }

    /// <summary>
    /// Subscribes to an empty event with a closure, action, and a predicate filter.
    /// </summary>
    /// <typeparam name="TClosure">The closure type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="closure">The closure value.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="eventFilter">Predicate filter for event messages.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TClosure>(
        this IEventSubscriber<EmptyEvent> subscriber, 
        TClosure closure,
        Action<TClosure> action, 
        Func<EmptyEvent, bool> eventFilter) 
    {
        return subscriber.Subscribe(
            new EmptyClosureEventObserver<TClosure>(closure, action),
            new PredicateEventFilter<EmptyEvent>(eventFilter)
        );
    }

    /// <summary>
    /// Subscribes to an empty event with a closure, action, and predicate filters for event, error, and completion.
    /// </summary>
    /// <typeparam name="TClosure">The closure type.</typeparam>
    /// <param name="subscriber">The event subscriber.</param>
    /// <param name="closure">The closure value.</param>
    /// <param name="action">The action to invoke on event.</param>
    /// <param name="onError">The action to invoke on error.</param>
    /// <param name="onComplete">The action to invoke on completion.</param>
    /// <param name="eventFilter">Predicate filter for event messages.</param>
    /// <param name="errorFilter">Predicate filter for errors.</param>
    /// <param name="completedFilter">Predicate filter for completion.</param>
    /// <returns>An IDisposable representing the subscription.</returns>
    public static IDisposable Subscribe<TClosure>(
        this IEventSubscriber<EmptyEvent> subscriber, 
        TClosure closure,
        Action<TClosure> action, 
        Action<TClosure, Exception>? onError = null, 
        Action<TClosure>? onComplete = null,
        Func<EmptyEvent, bool>? eventFilter = null, 
        Func<Exception, bool>? errorFilter = null,
        Func<bool>? completedFilter = null) 
    {
        return subscriber.Subscribe(
            new EmptyClosureEventObserver<TClosure>(closure, action, onError, onComplete),
            new PredicateEventFilter<EmptyEvent>(eventFilter, errorFilter, completedFilter)
        );
    }

    /// <summary>
    /// Publishes an empty event (without arguments).
    /// </summary>
    /// <param name="subscriber">The event publisher.</param>
    public static void Publish(this IEventPublisher<EmptyEvent> subscriber) {
        subscriber.Publish(new EmptyEvent());
    }
}