namespace DisposableEvents.Disposables;

/// <summary>
/// Provides extension methods for disposables to simplify adding them to collections.
/// </summary>
public static class DisposableExtensions {
    /// <summary>
    /// Adds the disposable to the specified bag and returns the disposable for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of the disposable.</typeparam>
    /// <param name="disposable">The disposable to add.</param>
    /// <param name="bag">The bag to add the disposable to.</param>
    /// <returns>The disposable that was added.</returns>
    public static T AddTo<T>(this T disposable, ref DisposableBag bag) where T : IDisposable {
        bag.Add(disposable);
        return disposable;
    }

    /// <summary>
    /// Adds the disposable to the specified builder and returns the disposable for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of the disposable.</typeparam>
    /// <param name="disposable">The disposable to add.</param>
    /// <param name="builder">The builder to add the disposable to.</param>
    /// <returns>The disposable that was added.</returns>
    public static T AddTo<T>(this T disposable, ref DisposableBuilder builder) where T : IDisposable {
        builder.Add(disposable);
        return disposable;
    }

    /// <summary>
    /// Adds the disposable to the specified collection and returns the disposable for method chaining.
    /// </summary>
    /// <typeparam name="T">The type of the disposable.</typeparam>
    /// <param name="disposable">The disposable to add.</param>
    /// <param name="collection">The collection to add the disposable to.</param>
    /// <returns>The disposable that was added.</returns>
    public static T AddTo<T>(this T disposable, ICollection<IDisposable> collection) where T : IDisposable {
        collection.Add(disposable);
        return disposable;
    }
}