namespace UnitTests;

internal class TestObserver<T> : IObserver<T>
{
    public readonly List<T> Received = new();
    public T LastValue => Received.Count > 0 ? Received[^1] : default!;
    public Exception? Error;
    public bool Completed;

    public void OnNext(T value) => Received.Add(value);
    public void OnError(Exception error) => Error = error;
    public void OnCompleted() => Completed = true;
}

internal class ThrowingObserver<T> : IObserver<T> {
    public Exception? Error;
    readonly Exception toThrow;

    public ThrowingObserver(Exception ex) {
        toThrow = ex;
    }

    public void OnNext(T value) => throw toThrow;
    public void OnError(Exception error) => Error = error;
    public void OnCompleted() { }
}