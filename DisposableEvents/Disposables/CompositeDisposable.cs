using System.Collections;

namespace DisposableEvents.Disposables;

public sealed class CompositeDisposable : ICollection<IDisposable>, IDisposable {
    readonly List<IDisposable> disposables;

    public bool IsDisposed { get; private set; }

    public CompositeDisposable(int capacity = 0) {
        if (capacity < 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity cannot be negative.");
        
        disposables = new List<IDisposable>(capacity);
    }

    public CompositeDisposable(params IDisposable[] disposables) {
        this.disposables = new List<IDisposable>(disposables);
    }
    public CompositeDisposable(IEnumerable<IDisposable> disposables) {
        this.disposables = new List<IDisposable>(disposables);
    }

    public void Add(IDisposable item) {
        if (IsDisposed) {
            item.Dispose();
            return;
        }
        disposables.Add(item);
    }

    public void Clear() {
        foreach (var disposable in disposables) {
            disposable.Dispose();
        }
        disposables.Clear();
    }

    public void Dispose() {
        if (IsDisposed) return;
        Clear();
        IsDisposed = true;
    }

    public int Count => disposables.Count;

    public bool IsReadOnly => false;

    public bool Contains(IDisposable item) => disposables.Contains(item);

    public void CopyTo(IDisposable[] array, int arrayIndex) => disposables.CopyTo(array, arrayIndex);

    public IEnumerator<IDisposable> GetEnumerator() => disposables.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Remove(IDisposable item) => disposables.Remove(item);
}