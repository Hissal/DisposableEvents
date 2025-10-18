namespace DisposableEvents.Internal;

internal sealed class FreeList<T> : IDisposable
    where T : class {
    const int c_defaultInitialCapacity = 4;
    const int c_minShrinkStart = 8;

    T?[] values = null!;
    int count;
    FastQueue<int> freeIndex = null!;
    bool isDisposed;
    readonly object gate = new object();

    public FreeList(int initialCapacity = c_defaultInitialCapacity) {
        Initialize();
    }
    // [MemberNotNull(nameof(freeIndex), nameof(values))]
    void Initialize() {
        freeIndex = new FastQueue<int>(c_defaultInitialCapacity);
        for (int i = 0; i < c_defaultInitialCapacity; i++) {
            freeIndex.Enqueue(i);
        }

        count = 0;

        var newValues = new T?[c_defaultInitialCapacity];
        Volatile.Write(ref values, newValues);
    }

    public T?[] GetValues() => values;
    public T? GetValue(int index) => values[index];
    
    public int GetCount() {
        lock (gate) {
            return count;
        }
    }

    public int Add(T value) {
        lock (gate) {
            if (isDisposed) throw new ObjectDisposedException(nameof(FreeList<T>));

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
    }

    public void Remove(int index, bool shrinkWhenEmpty) {
        lock (gate) {
            if (isDisposed) return; // do nothing

            ref var v = ref values[index];
            if (v == null) throw new KeyNotFoundException($"key index {index} is not found.");

            v = null;
            freeIndex.Enqueue(index);
            count--;

            if (shrinkWhenEmpty && count == 0 && values.Length > c_minShrinkStart) {
                Initialize(); // re-init.
            }
        }
    }

    public void Clear() {
        lock (gate) {
            if (isDisposed) 
                return;
            Initialize();
        }
    }

    public void Dispose() {
        lock (gate) {
            if (isDisposed) 
                return;
            isDisposed = true;

            freeIndex = null!;
            values = Array.Empty<T?>();
            count = 0;
        }
    }
}