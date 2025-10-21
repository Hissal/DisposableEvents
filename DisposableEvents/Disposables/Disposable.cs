namespace DisposableEvents.Disposables;

public static class Disposable {
    public static IDisposable Empty { get; } = new EmptyDisposable();

    sealed class EmptyDisposable : IDisposable {
        public void Dispose() { }
    }
    
    public static DisposableBuilder CreateBuilder() => new DisposableBuilder();
    public static DisposableBag CreateBag(int capacity) => new DisposableBag(capacity);

    public static IDisposable Action(Action onDispose) => new ActionDisposable(onDispose);
    public static IDisposable Action<TState>(TState state, Action<TState> onDispose) => new ActionDisposable<TState>(state, onDispose);
    
    public static BooleanDisposable Boolean() => new BooleanDisposable();

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2) =>
        new CombinedDisposable2(disposable1, disposable2);

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) =>
        new CombinedDisposable3(disposable1, disposable2, disposable3);

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4) =>
        new CombinedDisposable4(disposable1, disposable2, disposable3, disposable4);

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5) =>
        new CombinedDisposable5(disposable1, disposable2, disposable3, disposable4, disposable5);

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6) {
        return new CombinedDisposable6(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6);
    }

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7) {
        return new CombinedDisposable7(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6,
            disposable7);
    }

    public static IDisposable Combine(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6, IDisposable disposable7,
        IDisposable disposable8) {
        return new CombinedDisposable8(disposable1, disposable2, disposable3, disposable4, disposable5, disposable6,
            disposable7, disposable8);
    }

    public static IDisposable Combine(params IDisposable[] disposables) =>
        new CombinedDisposable(disposables);

    public static void Dispose(IDisposable disposable1, IDisposable disposable2) {
        disposable1.Dispose();
        disposable2.Dispose();
    }

    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
    }

    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
    }

    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
    }

    public static void Dispose(IDisposable disposable1, IDisposable disposable2, IDisposable disposable3,
        IDisposable disposable4, IDisposable disposable5, IDisposable disposable6) {
        disposable1.Dispose();
        disposable2.Dispose();
        disposable3.Dispose();
        disposable4.Dispose();
        disposable5.Dispose();
        disposable6.Dispose();
    }

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