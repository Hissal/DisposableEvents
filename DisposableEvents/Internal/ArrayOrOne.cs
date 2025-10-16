namespace DisposableEvents.Internal;

// TODO: Benchmark to see if better than array of one element
internal class ArrayOrOne<T> {
    readonly T[]? array;
    readonly T? one;
    readonly bool isArray;

    public ArrayOrOne(T[] array) {
        this.array = array;
        isArray = true;
    }

    public ArrayOrOne(T one) {
        this.one = one;
        isArray = false;
    }

    public int Length => isArray ? array!.Length : 1;
    public T this[int index] => isArray ? array![index] : (index == 0 ? one! : throw new IndexOutOfRangeException());
    
    public T[] AsArray() => isArray ? array! : new[] { one! };
}