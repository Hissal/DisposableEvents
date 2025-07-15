namespace DisposableEvents.Disposables;

public struct DisposableBag : IDisposable {
    IDisposable[]? disposables;
    bool isDisposed;
    int count;

    public DisposableBag(int capacity) {
        disposables = new IDisposable[capacity];
    }

    public void Add(IDisposable item) {
        if (isDisposed) {
            item.Dispose();
            return;
        }

        if (disposables == null) {
            disposables = new IDisposable[4];
        }
        else if (count == disposables.Length) {
            Array.Resize(ref disposables, count * 2);
        }

        disposables[count++] = item;
    }

    public void Clear() {
        if (disposables == null)
            return;

        for (int i = 0; i < count; i++) {
            disposables[i]?.Dispose();
        }

        disposables = null;
        count = 0;
    }

    public void Dispose() {
        if (isDisposed)
            return;
        
        Clear();
        isDisposed = true;
    }
}