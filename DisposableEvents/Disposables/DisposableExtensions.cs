namespace DisposableEvents.Disposables;

public static class DisposableExtensions {
    public static T AddTo<T>(this T disposable, ref DisposableBag bag) where T : IDisposable {
        bag.Add(disposable);
        return disposable;
    }

    public static T AddTo<T>(this T disposable, ref DisposableBuilder builder) where T : IDisposable {
        builder.Add(disposable);
        return disposable;
    }

    public static T AddTo<T>(this T disposable, ICollection<IDisposable> collection) where T : IDisposable {
        collection.Add(disposable);
        return disposable;
    }
}