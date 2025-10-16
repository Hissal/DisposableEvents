namespace DisposableEvents.Internal;

/// <summary>
/// Optional value that starts as "no value" and can later be set, for both value and reference types.
/// </summary>
/// <typeparam name="T">The stored value type.</typeparam>
internal struct Optional<T> {
    T _value;

    /// <summary>True when a value has been set.</summary>
    public bool HasValue { get; private set; }

    /// <summary>Gets the value or throws if not set.</summary>
    public T Value => HasValue ? _value : throw new InvalidOperationException("No value has been set.");

    /// <summary>Sets the value and marks it present.</summary>
    public void SetValue(T value) {
        _value = value;
        HasValue = true;
    }

    /// <summary>Clears the value and marks it absent.</summary>
    public void Clear() {
        HasValue = false;
        _value = default!;
    }

    /// <summary>Tries to get the value without throwing.</summary>
    public bool TryGet(out T value) {
        if (HasValue) {
            value = _value;
            return true;
        }

        value = default!;
        return false;
    }
}