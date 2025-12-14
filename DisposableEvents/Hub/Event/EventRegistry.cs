using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace DisposableEvents;

public enum EventRegistrationResult {
    Success,
    AlreadyRegistered
}

public sealed class EventRegistry : IDisposable {
    readonly ConcurrentDictionary<Type, IEventMarker> events = new();

#if NETSTANDARD2_0
    readonly object syncRoot = new();
#endif

    public EventRegistrationResult RegisterEvent<TMessage>(IDisposableEvent<TMessage> eventInstance) {
        return events.TryAdd(typeof(TMessage), eventInstance) 
            ? EventRegistrationResult.Success
            : EventRegistrationResult.AlreadyRegistered;
    }

    public IDisposableEvent<TMessage> GetEvent<TMessage>() {
        if (events.TryGetValue(typeof(TMessage), out var ev)) {
            return (IDisposableEvent<TMessage>)ev;
        }

        throw new KeyNotFoundException($"No event of type {typeof(TMessage)} is registered in the registry.");
    }
    
    public bool TryGetEvent<TMessage>([NotNullWhen(true)] out IDisposableEvent<TMessage>? eventInstance) {
        if (events.TryGetValue(typeof(TMessage), out var ev)) {
            eventInstance = (IDisposableEvent<TMessage>)ev;
            return true;
        }

        eventInstance = null;
        return false;
    }

    public IDisposableEvent<TMessage> GetOrAddEvent<TMessage>(Func<IDisposableEvent<TMessage>> factory) {
#if !NETSTANDARD2_0
        var ev = events.GetOrAdd(typeof(TMessage), static (_, f) => f(), factory);
        return (IDisposableEvent<TMessage>)ev;
#else
        lock (syncRoot) {
            if (events.TryGetValue(typeof(TMessage), out var existingEv)) {
                return (IDisposableEvent<TMessage>)existingEv;
            }
            
            var newEv = factory();
            events[typeof(TMessage)] = newEv;
            return newEv;
        }
#endif
    }
    public IDisposableEvent<TMessage> GetOrAddEvent<TState, TMessage>(TState state, Func<TState, IDisposableEvent<TMessage>> factory) {
#if !NETSTANDARD2_0
        var ev = events.GetOrAdd(typeof(TMessage), static (_, s) => s.factory(s.state), (state, factory));
        return (IDisposableEvent<TMessage>)ev;
#else
        lock (syncRoot) {
            if (events.TryGetValue(typeof(TMessage), out var existingEv)) {
                return (IDisposableEvent<TMessage>)existingEv;
            }
            
            var newEv = factory(state);
            events[typeof(TMessage)] = newEv;
            return newEv;
        }
#endif
    }

    public bool ContainsEvent<TMessage>() {
        return events.ContainsKey(typeof(TMessage));
    }

    public void Dispose() {
        foreach (var ev in events.Values) {
            ev.Dispose();
        }
        events.Clear();
    }
}