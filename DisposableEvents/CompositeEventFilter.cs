namespace DisposableEvents;

public enum FilterOrdering {
    /// <summary>
    /// Keeps the order of the filters as provided.
    /// </summary>
    KeepOriginal,
    /// <summary>
    /// Sorts the filters by their FilterOrder property, maintaining the relative order of filters with the same order value.
    /// </summary>
    /// <remarks>
    /// Stable sort is generally more expensive than <see cref="UnstableSort"/>, but preserves the order of filters with equal order values.
    /// </remarks>
    StableSort,
    /// <summary>
    /// Sorts the filters by their FilterOrder property, but does not guarantee the relative order of filters with the same order value.
    /// </summary>
    /// <remarks>
    /// Unstable sort is generally faster than <see cref="StableSort"/>, but does not preserve the order of filters with equal order values.
    /// </remarks>
    UnstableSort,
}

/// <summary>
/// Defines a composite event filter that applies multiple filters in order.
/// </summary>
/// <typeparam name="TMessage">The type of the event data.</typeparam>
public class CompositeEventFilter<TMessage> : IEventFilter<TMessage> {
    public int FilterOrder { get; }
    readonly IEventFilter<TMessage>[] orderedFilters;
    
    /// <summary>
    /// Creates a composite event filter from an array of filters, ordering them based on the specified ordering strategy.
    /// </summary>
    /// <typeparam name="TMessage">The type of the event data.</typeparam>
    /// <param name="filters">An array of filters to combine into a composite filter.</param>
    /// <param name="ordering">The ordering strategy to apply to the filters (e.g., KeepOriginal, StableSort, UnstableSort).</param>
    /// <param name="filterOrder">The order of the composite filter itself.</param>
    /// <returns>A new instance of <see cref="CompositeEventFilter{TMessage}"/> with the specified filters and ordering.</returns>
    /// /// <remarks>
    /// The <paramref name="filters"/> array is used directly and reordered in-place. <br/>
    /// If you need to preserve the original array, create a copy before passing it to this method.
    /// </remarks>
    public static CompositeEventFilter<TMessage> Create(IEventFilter<TMessage>[] filters, FilterOrdering ordering = FilterOrdering.StableSort, int filterOrder = 0) {
        ordering.OrderArray(filters);
        return new CompositeEventFilter<TMessage>(filters, filterOrder);
    }

    /// <summary>
    /// Creates a composite event filter from an enumerable collection of filters, ordering them based on the specified ordering strategy.
    /// </summary>
    /// <typeparam name="TMessage">The type of the event data.</typeparam>
    /// <param name="filters">An enumerable collection of filters to combine into a composite filter.</param>
    /// <param name="ordering">The ordering strategy to apply to the filters (e.g., KeepOriginal, StableSort, UnstableSort).</param>
    /// <param name="filterOrder">The order of the composite filter itself.</param>
    /// <returns>A new instance of <see cref="CompositeEventFilter{TMessage}"/> with the specified filters and ordering.</returns>
    public static CompositeEventFilter<TMessage> Create(IEnumerable<IEventFilter<TMessage>> filters, FilterOrdering ordering = FilterOrdering.StableSort, int filterOrder = 0) {
        var ordered = ordering.OrderToArray(filters);
        return new CompositeEventFilter<TMessage>(ordered, filterOrder);
    }
    
    /// <summary>
    /// Creates a composite event filter from a read-only span of filters, ordering them based on the specified ordering strategy.
    /// </summary>
    /// <typeparam name="TMessage">The type of the event data.</typeparam>
    /// <param name="filters">A read-only span of filters to combine into a composite filter.</param>
    /// <param name="ordering">The ordering strategy to apply to the filters (e.g., KeepOriginal, StableSort, UnstableSort).</param>
    /// <param name="filterOrder">The order of the composite filter itself.</param>
    /// <returns>A new instance of <see cref="CompositeEventFilter{TMessage}"/> with the specified filters and ordering.</returns>
    public static CompositeEventFilter<TMessage> Create(ReadOnlySpan<IEventFilter<TMessage>> filters, FilterOrdering ordering = FilterOrdering.StableSort, int filterOrder = 0) {
        return Create(filters.ToArray(), ordering, filterOrder);
    }
    
    CompositeEventFilter(IEventFilter<TMessage>[] orderedFilters, int filterOrder) {
        FilterOrder = filterOrder;
        this.orderedFilters = orderedFilters;
    }

    public FilterResult Filter(ref TMessage value) {
        foreach (var filter in orderedFilters) {
            if (filter.Filter(ref value).Blocked)
                return FilterResult.Block;
        }
        return FilterResult.Pass;
    }
}

public static class FilterOrderingExtensions {
    public static void OrderArray<T>(this FilterOrdering ordering, T[] filters) where T : IEventFilter {
        switch (ordering) {
            case FilterOrdering.KeepOriginal:
                return;
            case FilterOrdering.StableSort:
                StableInsertionSort(filters, static (x, y) => x.FilterOrder.CompareTo(y.FilterOrder));
                return;
            case FilterOrdering.UnstableSort:
                Array.Sort(filters, static (a, b) => a.FilterOrder.CompareTo(b.FilterOrder));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null);
        }
    }
    
    public static T[] OrderToArray<T>(this FilterOrdering ordering, IEnumerable<T> filters) where T : IEventFilter {
        switch (ordering) {
            case FilterOrdering.KeepOriginal:
                return filters.ToArray();
            case FilterOrdering.StableSort:
                return filters.OrderBy(f => f.FilterOrder).ToArray();
            case FilterOrdering.UnstableSort:
                var array = filters.ToArray();
                Array.Sort(array, static (a, b) => a.FilterOrder.CompareTo(b.FilterOrder));
                return array;
            default:
                throw new ArgumentOutOfRangeException(nameof(ordering), ordering, null);
        }
    }
    
    static void StableInsertionSort<T>(T[] array, Comparison<T> comparison) {
        for (int i = 1; i < array.Length; i++) {
            var key = array[i];
            int j = i - 1;
            while (j >= 0 && comparison(array[j], key) > 0) {
                array[j + 1] = array[j];
                j--;
            }
            array[j + 1] = key;
        }
    }
}