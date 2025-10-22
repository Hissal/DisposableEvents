using DisposableEvents.Internal;

namespace DisposableEvents;

public class LightEvent<TMessage> : IDisposableEvent<TMessage> {
    internal readonly LightList<IEventHandler<TMessage>> Handlers;

    public LightEvent() : this(GlobalConfig.InitialSubscriberCapacity) { }
    public LightEvent(int initialSubscriberCapacity) {
        Handlers = new LightList<IEventHandler<TMessage>>(initialSubscriberCapacity);
    }
    
    public void ClearSubscriptions() {
        throw new NotImplementedException();
    }

    public void Dispose() {
        Handlers.Dispose();
    }
    public bool IsDisposed { get; }
    public int HandlerCount { get; }

    public void Publish(TMessage message) {
        throw new NotImplementedException();
    }

    public IEventHandler<TMessage>[] GetHandlers() {
        throw new NotImplementedException();
    }

    public IDisposable Subscribe(IEventHandler<TMessage> handler) {
        throw new NotImplementedException();
    }
}

internal sealed class LightList<T> : IDisposable
    where T : class {
    const int c_defaultInitialCapacity = 4;
    const int c_minShrinkStart = 8;

    T?[] values = null!;
    int count;
    FastQueue<int> freeIndex = null!;
    bool IsDisposed => values.Length == 0;

    public LightList(int initialCapacity = c_defaultInitialCapacity) {
        Initialize(initialCapacity);
    }
    // [MemberNotNull(nameof(freeIndex), nameof(values))]
    void Initialize(int initialCapacity = c_defaultInitialCapacity) {
        freeIndex = new FastQueue<int>(initialCapacity);
        for (var i = 0; i < initialCapacity; i++) {
            freeIndex.Enqueue(i);
        }

        count = 0;

        var newValues = new T?[initialCapacity];
        Volatile.Write(ref values, newValues);
    }

    public T?[] GetValues() => values;
    public T? GetValue(int index) => values[index];
    
    public int GetCount() => count;

    public int Add(T value) {
        if (IsDisposed) 
            throw new ObjectDisposedException(nameof(FreeList<T>));

        if (freeIndex.Count != 0) {
            var index = freeIndex.Dequeue();
            values[index] = value;
            count++;
            return index;
        }
        else {
            // resize
            var newValues = new T[values.Length * 2];
            Array.Copy(values, 0, newValues, 0, values.Length);
            freeIndex.EnsureNewCapacity(newValues.Length);
            for (int i = values.Length; i < newValues.Length; i++) {
                freeIndex.Enqueue(i);
            }

            var index = freeIndex.Dequeue();
            newValues[values.Length] = value;
            count++;
            Volatile.Write(ref values, newValues);
            return index;
        }
    }

    public void Remove(int index, bool shrinkWhenEmpty) {
        if (IsDisposed) 
            return; // do nothing

        ref var v = ref values[index];
        if (v == null) throw new KeyNotFoundException($"key index {index} is not found.");

        v = null;
        freeIndex.Enqueue(index);
        count--;

        if (shrinkWhenEmpty && count == 0 && values.Length > c_minShrinkStart) {
            Initialize(); // re-init.
        }
    }

    public void Clear() {
        if (IsDisposed) 
            return;
        Initialize();
    }

    public void Dispose() {
        if (IsDisposed) 
            return;

        freeIndex = null!;
        values = Array.Empty<T?>();
        count = 0;
    }
}