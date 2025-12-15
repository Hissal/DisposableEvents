using System.Buffers;

namespace DisposableEvents.Disposables;

/// <summary>
/// A ref struct builder for efficiently constructing a composite disposable from multiple disposables.
/// Uses stack allocation for small numbers of disposables and array pooling for larger collections.
/// </summary>
public ref struct DisposableBuilder
#if NET9_0_OR_GREATER
    : IDisposable
#endif 
{
    IDisposable? disposable1;
    IDisposable? disposable2;
    IDisposable? disposable3;
    IDisposable? disposable4;
    IDisposable? disposable5;
    IDisposable? disposable6;
    IDisposable? disposable7;
    IDisposable? disposable8;
    IDisposable[]? disposables;

    int count;
    bool IsDisposed => count == -1;
    
    /// <summary>
    /// Adds a disposable to the builder. If already disposed, the disposable is disposed immediately.
    /// </summary>
    /// <param name="disposable">The disposable to add.</param>
    public void Add(IDisposable disposable) {
        if (IsDisposed) {
            disposable.Dispose();
            return;
        }

        switch (count) {
            case 0: disposable1 = disposable; break;
            case 1: disposable2 = disposable; break;
            case 2: disposable3 = disposable; break;
            case 3: disposable4 = disposable; break;
            case 4: disposable5 = disposable; break;
            case 5: disposable6 = disposable; break;
            case 6: disposable7 = disposable; break;
            case 7: disposable8 = disposable; break;
            default:
                AddBeyondEight(disposable);
                break;
        }
        
        count++;
    }

    void AddBeyondEight(IDisposable disposable) {
        if (count == 8) {
            var newDisposables = ArrayPool<IDisposable>.Shared.Rent(16);
            newDisposables[8] = disposable;
            newDisposables[0] = disposable1!;
            newDisposables[1] = disposable2!;
            newDisposables[2] = disposable3!;
            newDisposables[3] = disposable4!;
            newDisposables[4] = disposable5!;
            newDisposables[5] = disposable6!;
            newDisposables[6] = disposable7!;
            newDisposables[7] = disposable8!;
            disposable1 = disposable2 = disposable3 = disposable4 = disposable5 = disposable6 = disposable7 = disposable8 = null;
            disposables = newDisposables;
            return;
        }

        if (disposables!.Length == count) {
            var newDisposables = ArrayPool<IDisposable>.Shared.Rent(count * 2);
            Array.Copy(disposables, newDisposables, disposables.Length);
            ArrayPool<IDisposable>.Shared.Return(disposables, clearArray: true);
            disposables = newDisposables;
        }
        disposables[count] = disposable;
    }

    /// <summary>
    /// Builds the disposable collection and returns a single <see cref="IDisposable"/> that will dispose all added disposables.
    /// After calling this method, the builder is disposed and cannot be used again.
    /// </summary>
    /// <returns>A composite disposable that disposes all added disposables when disposed.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the builder has already been disposed.</exception>
    public IDisposable Build() {
        var result = count switch {
            -1 => Disposable.Empty,
            0 => Disposable.Empty,
            1 => disposable1!,
            2 => Disposable.Combine(disposable1!, disposable2!),
            3 => Disposable.Combine(disposable1!, disposable2!, disposable3!),
            4 => Disposable.Combine(disposable1!, disposable2!, disposable3!, disposable4!),
            5 => Disposable.Combine(disposable1!, disposable2!, disposable3!, disposable4!, disposable5!),
            6 => Disposable.Combine(disposable1!, disposable2!, disposable3!, disposable4!, disposable5!, disposable6!),
            7 => Disposable.Combine(disposable1!, disposable2!, disposable3!, disposable4!, disposable5!, disposable6!, disposable7!),
            8 => Disposable.Combine(disposable1!, disposable2!, disposable3!, disposable4!, disposable5!, disposable6!, disposable7!, disposable8!),
            _ => Disposable.Combine(disposables!.AsSpan(0, count).ToArray())
        };
        
        Dispose();
        return result;
    }

    /// <summary>
    /// Disposes the builder and releases any pooled arrays.
    /// After disposal, the builder cannot be used again.
    /// </summary>
    public void Dispose() {
        if (IsDisposed)
            return;
        
        disposable1 = disposable2 = disposable3 = disposable4 = disposable5 = disposable6 = disposable7 = disposable8 = null;
        if (disposables != null) {
            ArrayPool<IDisposable>.Shared.Return(disposables, clearArray: true);
        }
        count = -1; // Mark as disposed
    }
}