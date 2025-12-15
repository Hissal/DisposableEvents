using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DisposableEvents.Internal;

internal struct PooledArray<T>(T[] array, int length) : IDisposable {
    public static PooledArray<T> Empty { get; } = new ([], 0);
    public static PooledArray<T> Disposed { get; } = new (null!, 0);
    
    T[]? _array = array;
    
    /// <summary>
    /// Gets the underlying array. Throws if the object is disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when accessing the array after disposal.</exception>
    public T[] Array {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _array ?? throw new ObjectDisposedException(nameof(PooledArray<T>));
    }

    /// <summary>
    /// Gets the logical length of the pooled array.
    /// </summary>
    /// <remarks>
    /// This is the exact length that was requested when renting, not the actual length of the underlying
    /// array which may be larger due to pooling behavior. All operations (indexing, enumeration, etc.)
    /// respect this logical length.
    /// </remarks>
    public int Length { get; private set; } = length;

    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    [MemberNotNullWhen(false, nameof(_array))]
    public bool IsDisposed => _array == null;

    /// <summary>
    /// Gets a span representing the array contents.
    /// </summary>
    public Span<T> Span {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Array.AsSpan(0, Length);
    }
    
    /// <summary>
    /// Gets a memory representation of the array contents.
    /// </summary>
    public Memory<T> Memory {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Array.AsMemory(0, Length);
    }
    
    /// <summary>
    /// Gets an array segment representing the array contents.
    /// </summary>
    public ArraySegment<T> Segment {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => new(Array, 0, Length);
    }

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The index of the element.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if the index is out of range.</exception>
    public T this[int index] {
        get {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();
            return Array[index];
        }
        set {
            if (index < 0 || index >= Length)
                throw new IndexOutOfRangeException();
            Array[index] = value;
        }
    }
    
    /// <summary>
    /// Rents a pooled array of the specified length from <see cref="ArrayPool{T}.Shared"/>.
    /// </summary>
    /// <param name="length">The length of the array to rent.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> instance wrapping the rented array.</returns>
    /// <remarks>
    /// <para>
    /// The returned array must be disposed to return it to the pool. The underlying array's actual length
    /// may be larger than the requested length due to pooling behavior, but the <see cref="Length"/> property
    /// will return the exact requested length.
    /// </para>
    /// <para>
    /// All operations (indexing, spans, enumeration) respect the logical <see cref="Length"/>, not the
    /// underlying array's physical length.
    /// </para>
    /// </remarks>
    public static PooledArray<T> Rent(int length) {
        var array = ArrayPool<T>.Shared.Rent(length);
        return new PooledArray<T>(array, length);
    }

    /// <summary>
    /// Returns the pooled array to the pool.
    /// </summary>
    /// <param name="pooledArray">The pooled array to return.</param>
    /// <remarks>
    /// After calling this method, the pooled array is disposed and should not be used again.
    /// Do not call this method multiple times on the same array or copies of it.
    /// </remarks>
    public static void Return(ref PooledArray<T> pooledArray) => pooledArray.Dispose();
    
    /// <summary>
    /// Returns the pooled array to the pool, optionally clearing its contents.
    /// </summary>
    /// <param name="pooledArray">The pooled array to return.</param>
    /// <param name="clearArray">Whether to clear the array contents before returning to the pool.</param>
    /// <remarks>
    /// After calling this method, the pooled array is disposed and should not be used again.
    /// Do not call this method multiple times on the same array or copies of it.
    /// </remarks>
    public static void Return(ref PooledArray<T> pooledArray, bool clearArray) => pooledArray.ReturnToPool(clearArray);
    
    /// <summary>
    /// Creates an independent copy of the current pooled array with its own backing storage.
    /// </summary>
    /// <returns>A new <see cref="PooledArray{T}"/> containing a copy of the elements.</returns>
    /// <remarks>
    /// <para>
    /// Use this method when you want to share the data with another owner while retaining ownership
    /// of your original instance. Both the original and the copy have independent backing arrays and
    /// must be disposed independently.
    /// </para>
    /// <para>
    /// This differs from copying the struct value (passing by value), which transfers ownership by
    /// sharing the same underlying array. Use <see cref="RentCopy"/> when both the original and the
    /// copy need to remain valid and independently disposable.
    /// </para>
    /// </remarks>
    public PooledArray<T> RentCopy() => RentSlice(0, Length);
    
    /// <summary>
    /// Creates a slice of the current pooled array starting at the specified index.
    /// </summary>
    /// <param name="start">The starting index of the slice.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the sliced elements.</returns>
    /// <remarks>
    /// <para>
    /// This method creates an independent copy with its own backing array containing only the sliced elements.
    /// Both the original and the returned slice have separate backing arrays and must be disposed independently.
    /// </para>
    /// <para>
    /// The original array retains ownership of its backing storage. Use this method when you want to share
    /// a subset of the data while both instances remain valid and independently disposable.
    /// </para>
    /// </remarks>
    public PooledArray<T> RentSlice(int start) => RentSlice(start, Length - start);
    
    /// <summary>
    /// Creates a slice of the current pooled array starting at the specified index and with the specified length.
    /// </summary>
    /// <param name="start">The starting index of the slice.</param>
    /// <param name="length">The length of the slice.</param>
    /// <returns>A new <see cref="PooledArray{T}"/> containing the sliced elements.</returns>
    /// <remarks>
    /// <para>
    /// This method creates an independent copy with its own backing array containing only the sliced elements.
    /// Both the original and the returned slice have separate backing arrays and must be disposed independently.
    /// </para>
    /// <para>
    /// The original array retains ownership of its backing storage. Use this method when you want to share
    /// a subset of the data while both instances remain valid and independently disposable.
    /// </para>
    /// </remarks>
    public PooledArray<T> RentSlice(int start, int length) {
        var slice = Span.Slice(start, length);
        var newArr = Rent(length);
        slice.CopyTo(newArr.Span);
        return newArr;
    }
    
    /// <summary>
    /// Disposes the pooled array, returning it to the pool.
    /// </summary>
    /// <remarks>
    /// Arrays containing reference types or types with references are automatically cleared
    /// before being returned to the pool to avoid memory leaks.
    /// </remarks>
    public void Dispose() => ReturnToPool(RuntimeHelpers.IsReferenceOrContainsReferences<T>());

    void ReturnToPool(bool clearArray) {
        if (IsDisposed)
            return;
        
        ArrayPool<T>.Shared.Return(_array, clearArray);
        _array = null!;
        Length = 0;
    }
    
    /// <summary>
    /// Gets an enumerator for the pooled array.
    /// </summary>
    /// <returns>An enumerator for the pooled array.</returns>
    /// <remarks>
    /// The enumerator will iterate exactly <see cref="Length"/> elements, regardless of the underlying
    /// array's actual length which may be larger due to pooling.
    /// </remarks>
    public Enumerator GetEnumerator() => new (this);
    
    /// <summary>
    /// Represents an enumerator for the pooled array.
    /// </summary>
    public struct Enumerator(PooledArray<T> pooledArray) {
        int _index = -1;

        /// <summary>
        /// Gets the current element in the array.
        /// </summary>
        public T Current => pooledArray[_index];

        /// <summary>
        /// Advances the enumerator to the next element.
        /// </summary>
        /// <returns>True if the enumerator was successfully advanced; otherwise, false.</returns>
        public bool MoveNext() {
            _index++;
            return _index < pooledArray.Length;
        }
    }
}