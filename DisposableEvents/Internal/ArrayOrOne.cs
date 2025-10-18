using System.Collections;

namespace DisposableEvents.Internal;

// TODO: Cahce IEnumerable Enumerator
internal sealed class ArrayOrOne<T> : IEnumerable<T>, ICollection {
    readonly T? one;
    public T[]? Array { get; }

    public bool IsArray { get; }
    public bool IsOne => !IsArray;

    public T One => IsOne ? one! : Array![0];

    public ArrayOrOne(T[] array) {
        Array = array;
        IsArray = true;
    }

    public ArrayOrOne(T one) {
        this.one = one;
        IsArray = false;
    }

    public static implicit operator ArrayOrOne<T>(T[] array) => new ArrayOrOne<T>(array);
    public static implicit operator ArrayOrOne<T>(T one) => new ArrayOrOne<T>(one);

    public int Length => IsArray ? Array!.Length : 1;
    public T this[int index] => IsArray ? Array![index] : (index == 0 ? one! : throw new IndexOutOfRangeException());

    public T[] AsArray() => IsArray ? Array! : new[] { one! };

    // Non-allocating pattern-based enumerator used by 'foreach' when iterating ArrayOrOne<T> directly.
    public Enumerator GetEnumerator() => new Enumerator(this);

    public struct Enumerator {
        readonly T[]? array;
        readonly T? one;
        readonly bool isArray;
        int index;

        internal Enumerator(ArrayOrOne<T> source) {
            isArray = source.IsArray;
            if (isArray) {
                array = source.Array;
                one = default;
            }
            else {
                array = null;
                one = source.one;
            }

            index = -1;
        }

        public bool MoveNext() {
            index++;
            if (isArray) return array is not null && index < array.Length;
            return index == 0;
        }

        public T Current => isArray ? array![index] : one!;
    }

    // Explicit interface implementations (may allocate because they return IEnumerator<T>/IEnumerator).

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IEnumerable<T>)this).GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return IsArray
            ? ((IEnumerable<T>)Array!).GetEnumerator()
            : new SingleEnumerator(one!);
    }

    sealed class SingleEnumerator : IEnumerator<T> {
        readonly T value;
        int state; // -1 = before start, 0 = yielded, 1 = finished

        public SingleEnumerator(T value) {
            this.value = value;
            state = -1;
        }

        public T Current => state == 0 ? value : throw new InvalidOperationException();
        object IEnumerator.Current => Current;

        public bool MoveNext() {
            if (state == -1) {
                state = 0;
                return true;
            }

            state = 1;
            return false;
        }

        public void Reset() => state = -1;
        public void Dispose() { }
    }

    public void CopyTo(Array array, int index) {
        if (IsArray) {
            Array!.CopyTo(array, index);
        }
        else {
            array.SetValue(one!, index);
        }
    }

    public int Count => IsArray ? Array!.Length : 1;
    public bool IsSynchronized => false;
    public object SyncRoot => this;
}