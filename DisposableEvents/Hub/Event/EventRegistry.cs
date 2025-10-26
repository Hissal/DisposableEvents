using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace DisposableEvents;

public enum EventRegisterationResult {
    Added,
    AlreadyExists
}

public sealed class EventRegistry : IDisposable {
    readonly ConcurrentDictionary<Type, IEventMarker> events = new();

    public EventRegisterationResult RegisterEvent<TMessage>(IDisposableEvent<TMessage> eventInstance) {
        return events.TryAdd(typeof(TMessage), eventInstance) 
            ? EventRegisterationResult.Added
            : EventRegisterationResult.AlreadyExists;
    }

    public IDisposableEvent<TMessage>? GetEvent<TMessage>() {
        if (events.TryGetValue(typeof(TMessage), out var ev)) {
            return (IDisposableEvent<TMessage>)ev;
        }

        return null;
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
        var ev = events.GetOrAdd(typeof(TMessage), static (_, f) => f(), factory);
        return (IDisposableEvent<TMessage>)ev;
    }
    public IDisposableEvent<TMessage> GetOrAddEvent<TState, TMessage>(TState state, Func<TState, IDisposableEvent<TMessage>> factory) {
        var ev = events.GetOrAdd(typeof(TMessage), static (_, s) => s.factory(s.state), (state, factory));
        return (IDisposableEvent<TMessage>)ev;
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