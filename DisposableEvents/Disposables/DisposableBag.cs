namespace DisposableEvents.Disposables;

public class DisposableBag : IDisposable {
    readonly List<IDisposable> disposables;
    public DisposableBag() {
        disposables = new List<IDisposable>();
    }
    
    public void Add(IDisposable disposable) {
        if (disposable == null) throw new ArgumentNullException(nameof(disposable));
        disposables.Add(disposable);
    }
    
    public void Dispose() {
        foreach (var disposable in disposables) {
            disposable.Dispose();
        }
        disposables.Clear();
    }
}

public static class DisposableBagExtensions {
    public static void AddTo(this IDisposable disposable, DisposableBag bag) {
        if (disposable == null) throw new ArgumentNullException(nameof(disposable));
        if (bag == null) throw new ArgumentNullException(nameof(bag));
        bag.Add(disposable);
    }
}

public static class Disposable {
    static Lazy<EmptyDisposable> LazyEmpty { get; } = new Lazy<EmptyDisposable>(() => new EmptyDisposable());
    public static IDisposable Empty => LazyEmpty.Value;
}

public class EmptyDisposable : IDisposable {
    public void Dispose() {
        // No op
    }
}