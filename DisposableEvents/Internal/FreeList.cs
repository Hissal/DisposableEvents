namespace DisposableEvents.Internal;

/// <summary>
/// Identifies a slot handed out by <see cref="FreeList{T}"/>.
/// <para>
/// The generation separates keys handed out before a <see cref="FreeList{T}.Clear"/> from keys
/// pointing at the same index afterwards, so a stale key can never remove an unrelated value.
/// </para>
/// </summary>
internal readonly struct FreeListKey {
    public readonly int Index;
    public readonly int Generation;

    public FreeListKey(int index, int generation) {
        Index = index;
        Generation = generation;
    }
}

internal sealed class FreeList<T> : IDisposable
    where T : class {
    const int c_defaultInitialCapacity = 4;
    const int c_minShrinkStart = 8;

    T?[] values = null!;
    int count;
    int generation;
    FastQueue<int> freeIndex = null!;
    bool isDisposed;
    readonly object gate = new object();

    public FreeList(int initialCapacity = c_defaultInitialCapacity) {
        if (initialCapacity < 0) 
            throw new ArgumentOutOfRangeException(nameof(initialCapacity), "Capacity must be non-negative.");
        
        Initialize(initialCapacity);
    }
    // [MemberNotNull(nameof(freeIndex), nameof(values))]
    void Initialize(int initialCapacity = c_defaultInitialCapacity) {
        // Invalidates every key handed out so far.
        generation++;
        
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
    
    public int GetCount() {
        lock (gate) {
            return count;
        }
    }

    public FreeListKey Add(T value) {
        lock (gate) {
            if (isDisposed) throw new ObjectDisposedException(nameof(FreeList<T>));

            if (freeIndex.Count != 0) {
                var index = freeIndex.Dequeue();
                values[index] = value;
                count++;
                return new FreeListKey(index, generation);
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
                return new FreeListKey(index, generation);
            }
        }
    }

    /// <summary>
    /// Removes the value stored under <paramref name="key"/>.
    /// </summary>
    /// <returns>
    /// <c>true</c> when the value was removed, <c>false</c> when the key is stale (invalidated by
    /// <see cref="Clear"/> or by a shrink) or the list is disposed.
    /// </returns>
    /// <exception cref="KeyNotFoundException">
    /// The key belongs to the current generation but its slot is empty, which means the key was
    /// already removed rather than invalidated.
    /// </exception>
    public bool Remove(FreeListKey key, bool shrinkWhenEmpty) {
        lock (gate) {
            if (isDisposed) return false; // do nothing

            // Stale key, the slot it pointed at no longer belongs to it.
            if (key.Generation != generation) return false;

            ref var v = ref values[key.Index];
            if (v == null) throw new KeyNotFoundException($"key index {key.Index} is not found.");

            v = null;
            freeIndex.Enqueue(key.Index);
            count--;

            if (shrinkWhenEmpty && count == 0 && values.Length > c_minShrinkStart) {
                Initialize(); // re-init.
            }

            return true;
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
