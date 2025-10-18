using System.Diagnostics.CodeAnalysis;

namespace DisposableEvents.Internal;

/// <summary>
/// Optional value that starts as "no value" and can later be set, for both value and reference types.
/// </summary>
/// <typeparam name="T">The stored value type.</typeparam>
internal readonly struct Optional<T> {
    public readonly T? Value;

    /// <summary>True when a value has been set.</summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; init; }
    
    Optional(T? value, bool hasValue) {
        Value = value;
        HasValue = hasValue;
    }
    
    public static implicit operator Optional<T>(T value) => From(value);
    public static implicit operator T?(Optional<T> optional) => optional.Value;
    
    public static Optional<T> From(T value) => new Optional<T>(value, true);
    public static Optional<T> Null() => new Optional<T>(default, false);

    /// <summary>Tries to get the value without throwing.</summary>
    public bool TryGetValue([NotNullWhen(true)] out T? value) {
        if (HasValue) {
            value = Value;
            return true;
        }

        value = default;
        return false;
    }
}