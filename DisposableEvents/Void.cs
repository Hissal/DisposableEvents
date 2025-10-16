namespace DisposableEvents;

/// <summary>
/// Represents a void-like structure that can be used as a placeholder for methods or events 
/// that do not require a meaningful return value or message.
/// </summary>
public readonly record struct Void {
    /// <summary>
    /// A static instance of the <see cref="Void"/> struct, representing a default or singleton value.
    /// </summary>
    public static readonly Void Value = new Void();
}