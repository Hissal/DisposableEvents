namespace DisposableEvents.Disposables;

/// <summary>
/// Provides factory methods and utilities for creating and managing disposable resources.
/// </summary>
public static class Disposable {
    /// <summary>
    /// Gets an empty disposable that does nothing when disposed.
    /// </summary>
    public static IDisposable Empty { get; } = new EmptyDisposable();

    sealed class EmptyDisposable : IDisposable {
        public void Dispose() { }
    }
    
    /// <summary>
    /// Creates a new <see cref="DisposableBuilder"/> for efficiently building composite disposables.
    /// </summary>
    /// <returns>A new disposable builder instance.</returns>
    public static DisposableBuilder CreateBuilder() => new DisposableBuilder();
    
    /// <summary>
    /// Creates a new <see cref="DisposableBag"/> with the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the bag.</param>
    /// <returns>A new disposable bag instance.</returns>
    public static DisposableBag CreateBag(int capacity) => new DisposableBag(capacity);

    /// <summary>
    /// Creates a disposable that invokes the specified action when disposed.
    /// </summary>
    /// <param name="onDispose">The action to invoke on disposal.</param>
    /// <returns>A disposable that executes the action when disposed.</returns>
    public static IDisposable Action(Action onDispose) => new ActionDisposable(onDispose);
    
    /// <summary>
    /// Creates a disposable that invokes the specified action with state when disposed.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The state to pass to the action.</param>
    /// <param name="onDispose">The action to invoke on disposal.</param>
    /// <returns>A disposable that executes the action with state when disposed.</returns>
    public static IDisposable Action<TState>(TState state, Action<TState> onDispose) => new ActionDisposable<TState>(state, onDispose);
    
    /// <summary>
    /// Creates a new <see cref="BooleanDisposable"/> that tracks its disposal state.
    /// </summary>
    /// <returns>A new boolean disposable instance.</returns>
    public static BooleanDisposable Boolean() => new BooleanDisposable();

    /// <summary>
    /// Combines two disposables into a single disposable that disposes both when disposed.
    /// </summary>
    /// <param name="disposable1">The first disposable.</param>
    /// <param name="disposable2">The second disposable.</param>
    /// <returns>A composite disposable.</returns>
    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2) =>
        new CombinedDisposable2(disposable1, disposable2);

    /// <summary>
    /// Combines three disposables into a single disposable that disposes all when disposed.
    /// </summary>
    /// <param name="disposable1">The first disposable.</param>
    /// <param name="disposable2">The second disposable.</param>
    /// <param name="disposable3">The third disposable.</param>
    /// <returns>A composite disposable.</returns>
    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) =>
        new CombinedDisposable3(disposable1, disposable2, disposable3);

    /// <summary>
    /// Combines four disposables into a single disposable that disposes all when disposed.
    /// </summary>
    /// <param name="disposable1">The first disposable.</param>
    /// <param name="disposable2">The second disposable.</param>
    /// <param name="disposable3">The third disposable.</param>
    /// <param name="disposable4">The fourth disposable.</param>
    /// <returns>A composite disposable.</returns>
    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4) =>
        new CombinedDisposable4(disposable1, disposable2, disposable3, disposable4);

    /// <summary>
    /// Combines five disposables into a single disposable that disposes all when disposed.
    /// </summary>
    /// <param name="disposable1">The first disposable.</param>
    /// <param name="disposable2">The second disposable.</param>
    /// <param name="disposable3">The third disposable.</param>
    /// <param name="disposable4">The fourth disposable.</param>
    /// <param name="disposable5">The fifth disposable.</param>
    /// <returns>A composite disposable.</returns>
    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5) =>
        new CombinedDisposable5(disposable1, disposable2, disposable3, disposable4, disposable5);

    /// <summary>
    /// Combines six disposables into a single disposable that disposes all when disposed.
    /// </summary>
    /// <param name="disposable1">The first disposable.</param>
    /// <param name="disposable2">The second disposable.</param>
    /// <param name="disposable3">The third disposable.</param>
    /// <param name="disposable4">The fourth disposable.</param>
    /// <param name="disposable5">The fifth disposable.</param>
    /// <param name="disposable6">The sixth disposable.</param>
    /// <returns>A composite disposable.</returns>
    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6) {
        return new CombinedDisposable6(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6);
    }

    /// <summary>
    /// Combines seven disposables into a single disposable that disposes all when disposed.
    /// </summary>
    /// <param name="disposable1">The first disposable.</param>
    /// <param name="disposable2">The second disposable.</param>
    /// <param name="disposable3">The third disposable.</param>
    /// <param name="disposable4">The fourth disposable.</param>
    /// <param name="disposable5">The fifth disposable.</param>
    /// <param name="disposable6">The sixth disposable.</param>
    /// <param name="disposable7">The seventh disposable.</param>
    /// <returns>A composite disposable.</returns>
    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7) {
        return new CombinedDisposable7(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6,
            disposable7);
    }

    /// <summary>
    /// Combines eight disposables into a single disposable that disposes all when disposed.
    /// </summary>
    /// <param name="disposable1">The first disposable.</param>
    /// <param name="disposable2">The second disposable.</param>
    /// <param name="disposable3">The third disposable.</param>
    /// <param name="disposable4">The fourth disposable.</param>
    /// <param name="disposable5">The fifth disposable.</param>
    /// <param name="disposable6">The sixth disposable.</param>
    /// <param name="disposable7">The seventh disposable.</param>
    /// <param name="disposable8">The eighth disposable.</param>
    /// <returns>A composite disposable.</returns>
    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7,
        IDisposable disposable8) {
        return new CombinedDisposable8(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6,
            disposable7, disposable8);
    }

    /// <summary>
    /// Combines multiple disposables into a single disposable that disposes all when disposed.
    /// </summary>
    /// <param name="disposables">An array of disposables to combine.</param>
    /// <returns>A composite disposable.</returns>
    public static IDisposable Combine(params IDisposable[] disposables) =>
        new CombinedDisposable(disposables);

    /// <summary>
    /// Disposes two disposables in sequence.
    /// </summary>
    /// <param name="disposable1">The first disposable to dispose.</param>
    /// <param name="disposable2">The second disposable to dispose.</param>
    public static void Dispose(IDisposable disposable1, IDisposable disposable2) {
        disposable1.Dispose();
        disposable2.Dispose();
    }

    /// <summary>
    /// Disposes three disposables in sequence.
    /// </summary>
    /// <param name="disposable1">The first disposable to dispose.</param>
    /// <param name="disposable2">The second disposable to dispose.</param>
    /// <param name="disposable3">The third disposable to dispose.</param>
    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
    }

    /// <summary>
    /// Disposes four disposables in sequence.
    /// </summary>
    /// <param name="disposable1">The first disposable to dispose.</param>
    /// <param name="disposable2">The second disposable to dispose.</param>
    /// <param name="disposable3">The third disposable to dispose.</param>
    /// <param name="disposable4">The fourth disposable to dispose.</param>
    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
    }

    /// <summary>
    /// Disposes five disposables in sequence.
    /// </summary>
    /// <param name="disposable1">The first disposable to dispose.</param>
    /// <param name="disposable2">The second disposable to dispose.</param>
    /// <param name="disposable3">The third disposable to dispose.</param>
    /// <param name="disposable4">The fourth disposable to dispose.</param>
    /// <param name="disposable5">The fifth disposable to dispose.</param>
    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
    }

    /// <summary>
    /// Disposes six disposables in sequence.
    /// </summary>
    /// <param name="disposable1">The first disposable to dispose.</param>
    /// <param name="disposable2">The second disposable to dispose.</param>
    /// <param name="disposable3">The third disposable to dispose.</param>
    /// <param name="disposable4">The fourth disposable to dispose.</param>
    /// <param name="disposable5">The fifth disposable to dispose.</param>
    /// <param name="disposable6">The sixth disposable to dispose.</param>
    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
    }

    /// <summary>
    /// Disposes seven disposables in sequence.
    /// </summary>
    /// <param name="disposable1">The first disposable to dispose.</param>
    /// <param name="disposable2">The second disposable to dispose.</param>
    /// <param name="disposable3">The third disposable to dispose.</param>
    /// <param name="disposable4">The fourth disposable to dispose.</param>
    /// <param name="disposable5">The fifth disposable to dispose.</param>
    /// <param name="disposable6">The sixth disposable to dispose.</param>
    /// <param name="disposable7">The seventh disposable to dispose.</param>
    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
        disposable7.Dispose();
    }

    /// <summary>
    /// Disposes eight disposables in sequence.
    /// </summary>
    /// <param name="disposable1">The first disposable to dispose.</param>
    /// <param name="disposable2">The second disposable to dispose.</param>
    /// <param name="disposable3">The third disposable to dispose.</param>
    /// <param name="disposable4">The fourth disposable to dispose.</param>
    /// <param name="disposable5">The fifth disposable to dispose.</param>
    /// <param name="disposable6">The sixth disposable to dispose.</param>
    /// <param name="disposable7">The seventh disposable to dispose.</param>
    /// <param name="disposable8">The eighth disposable to dispose.</param>
    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7,
        IDisposable disposable8) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
        disposable7.Dispose();
        disposable8.Dispose();
    }

    /// <summary>
    /// Disposes all disposables in the array in sequence.
    /// </summary>
    /// <param name="disposables">An array of disposables to dispose.</param>
    public static void Dispose(params IDisposable[] disposables) {
        foreach (var disposable in disposables) {
            disposable.Dispose();
        }
    }

    sealed class CombinedDisposable2 : IDisposable {
        readonly IDisposable disposable1;
        readonly IDisposable disposable2;

        public CombinedDisposable2(IDisposable disposable1, IDisposable disposable2) {
            this.disposable1 = disposable1;
            this.disposable2 = disposable2;
        }

        public void Dispose() {
            disposable1.Dispose();
            disposable2.Dispose();
        }
    }

    sealed class CombinedDisposable3 : IDisposable {
        readonly IDisposable disposable1;
        readonly IDisposable disposable2;
        readonly IDisposable disposable3;

        public CombinedDisposable3(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) {
            this.disposable1 = disposable1;
            this.disposable2 = disposable2;
            this.disposable3 = disposable3;
        }

        public void Dispose() {
            disposable1.Dispose();
            disposable2.Dispose();
            disposable3.Dispose();
        }
    }

    sealed class CombinedDisposable4 : IDisposable {
        readonly IDisposable disposable1;
        readonly IDisposable disposable2;
        readonly IDisposable disposable3;
        readonly IDisposable disposable4;

        public CombinedDisposable4(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4) {
            this.disposable1 = disposable1;
            this.disposable2 = disposable2;
            this.disposable3 = disposable3;
            this.disposable4 = disposable4;
        }

        public void Dispose() {
            disposable1.Dispose();
            disposable2.Dispose();
            disposable3.Dispose();
            disposable4.Dispose();
        }
    }

    sealed class CombinedDisposable5 : IDisposable {
        readonly IDisposable disposable1;
        readonly IDisposable disposable2;
        readonly IDisposable disposable3;
        readonly IDisposable disposable4;
        readonly IDisposable disposable5;

        public CombinedDisposable5(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4, IDisposable disposable5) {
            this.disposable1 = disposable1;
            this.disposable2 = disposable2;
            this.disposable3 = disposable3;
            this.disposable4 = disposable4;
            this.disposable5 = disposable5;
        }

        public void Dispose() {
            disposable1.Dispose();
            disposable2.Dispose();
            disposable3.Dispose();
            disposable4.Dispose();
            disposable5.Dispose();
        }
    }

    sealed class CombinedDisposable6 : IDisposable {
        readonly IDisposable disposable1;
        readonly IDisposable disposable2;
        readonly IDisposable disposable3;
        readonly IDisposable disposable4;
        readonly IDisposable disposable5;
        readonly IDisposable disposable6;

        public CombinedDisposable6(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4, IDisposable disposable5, IDisposable disposable6) {
            this.disposable1 = disposable1;
            this.disposable2 = disposable2;
            this.disposable3 = disposable3;
            this.disposable4 = disposable4;
            this.disposable5 = disposable5;
            this.disposable6 = disposable6;
        }

        public void Dispose() {
            disposable1.Dispose();
            disposable2.Dispose();
            disposable3.Dispose();
            disposable4.Dispose();
            disposable5.Dispose();
            disposable6.Dispose();
        }
    }

    sealed class CombinedDisposable7 : IDisposable {
        readonly IDisposable disposable1;
        readonly IDisposable disposable2;
        readonly IDisposable disposable3;
        readonly IDisposable disposable4;
        readonly IDisposable disposable5;
        readonly IDisposable disposable6;
        readonly IDisposable disposable7;

        public CombinedDisposable7(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7) {
            this.disposable1 = disposable1;
            this.disposable2 = disposable2;
            this.disposable3 = disposable3;
            this.disposable4 = disposable4;
            this.disposable5 = disposable5;
            this.disposable6 = disposable6;
            this.disposable7 = disposable7;
        }

        public void Dispose() {
            disposable1.Dispose();
            disposable2.Dispose();
            disposable3.Dispose();
            disposable4.Dispose();
            disposable5.Dispose();
            disposable6.Dispose();
            disposable7.Dispose();
        }
    }

    sealed class CombinedDisposable8 : IDisposable {
        readonly IDisposable disposable1;
        readonly IDisposable disposable2;
        readonly IDisposable disposable3;
        readonly IDisposable disposable4;
        readonly IDisposable disposable5;
        readonly IDisposable disposable6;
        readonly IDisposable disposable7;
        readonly IDisposable disposable8;

        public CombinedDisposable8(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
            IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7,
            IDisposable disposable8) {
            this.disposable1 = disposable1;
            this.disposable2 = disposable2;
            this.disposable3 = disposable3;
            this.disposable4 = disposable4;
            this.disposable5 = disposable5;
            this.disposable6 = disposable6;
            this.disposable7 = disposable7;
            this.disposable8 = disposable8;
        }

        public void Dispose() {
            disposable1.Dispose();
            disposable2.Dispose();
            disposable3.Dispose();
            disposable4.Dispose();
            disposable5.Dispose();
            disposable6.Dispose();
            disposable7.Dispose();
            disposable8.Dispose();
        }
    }

    sealed class CombinedDisposable : IDisposable {
        readonly IDisposable[] disposables;

        public CombinedDisposable(IDisposable[] disposables) {
            this.disposables = disposables;
        }

        public void Dispose() {
            foreach (var disposable in disposables) {
                disposable?.Dispose();
            }
        }
    }

    sealed class ActionDisposable : IDisposable {
        Action? onDispose;

        public ActionDisposable(Action onDispose) {
            this.onDispose = onDispose;
        }

        public void Dispose() {
            Interlocked.Exchange(ref onDispose, null)?.Invoke();
        }
    }

    sealed class ActionDisposable<TState> : IDisposable {
        Action<TState>? onDispose;
        TState state;

        public ActionDisposable(TState state, Action<TState> onDispose) {
            this.onDispose = onDispose;
            this.state = state;
        }

        public void Dispose() {
            Interlocked.Exchange(ref onDispose, null)?.Invoke(state);
            state = default!;
        }
    }
}