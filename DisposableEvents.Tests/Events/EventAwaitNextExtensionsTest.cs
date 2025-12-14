using System.Diagnostics;

namespace DisposableEvents.Tests.Events;

[TestSubject(typeof(EventAwaitNextExtensions))]
public class EventAwaitNextExtensionsTest {
    readonly DisposableEvent<int> evt = new ();
    readonly IEventFilter<int>[] filters = Enumerable.Range(0, 2)
        .Select(_ => Substitute.For<IEventFilter<int>>())
        .ToArray();
    
    readonly Stopwatch stopwatch = new();
    const int c_delayMs = 100;
    
    [Fact]
    public async Task AwaitNextAsync_NoFilter_Works() {
        stopwatch.Start();
        
        _ = Task.Run(async () => {
            await Task.Delay(c_delayMs);
            evt.Publish(7);
        }, TestContext.Current.CancellationToken);
        
        var result = await evt.AwaitNextAsync(CancellationToken.None);
        stopwatch.Stop();
        
        result.Should().Be(7);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(c_delayMs);
    }
    
    [Fact]
    public async Task AwaitNextAsync_OneFilter_Works() {
        var filter = filters[0];
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        
        stopwatch.Start();
        
        _ = Task.Run(async () => {
            await Task.Delay(c_delayMs);
            evt.Publish(7);
            filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
            await Task.Delay(c_delayMs);
            evt.Publish(7);
        });
        
        var result = await evt.AwaitNextAsync(filter, CancellationToken.None);
        stopwatch.Stop();
        
        result.Should().Be(7);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(c_delayMs * 2);
    }
    
    [Fact]
    public async Task AwaitNextAsync_MultipleFilters_Works() {
        var filter1 = filters[0];
        var filter2 = filters[1];
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        
        stopwatch.Start();
        
        _ = Task.Run(async () => {
            await Task.Delay(c_delayMs);
            evt.Publish(7);
            filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
            await Task.Delay(c_delayMs);
            evt.Publish(7);
            filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
            await Task.Delay(c_delayMs);
            evt.Publish(7);
        });
        
        var result = await evt.AwaitNextAsync(filters, cancellationToken: CancellationToken.None);
        stopwatch.Stop();
        
        result.Should().Be(7);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(c_delayMs * 3);
    }
    
    [Fact]
    public async Task AwaitNextAsync_ParamsFilters_Works() {
        var filter1 = filters[0];
        var filter2 = filters[1];
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        
        stopwatch.Start();
        
        _ = Task.Run(async () => {
            await Task.Delay(c_delayMs);
            evt.Publish(7);
            filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
            await Task.Delay(c_delayMs);
            evt.Publish(7);
            filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
            await Task.Delay(c_delayMs);
            evt.Publish(7);
        });
        
        var result = await evt.AwaitNextAsync(CancellationToken.None, filter1, filter2);
        stopwatch.Stop();
        
        result.Should().Be(7);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(c_delayMs * 3);
    }
    
    // ===== Cancellation ===== //
    [Fact]
    public async Task AwaitNextAsync_NoFilter_CanBeCancelled() {
        stopwatch.Start();
        
        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () => {
            await Task.Delay(c_delayMs);
            cts.Cancel();
        }, TestContext.Current.CancellationToken);
        
        await FluentActions.Awaiting(() => evt.AwaitNextAsync(cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
        
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(c_delayMs);
    }

    [Fact]
    public async Task AwaitNextAsync_OneFilter_CanBeCancelled() {
        var filter = filters[0];
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        
        stopwatch.Start();
        
        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () => {
            await Task.Delay(c_delayMs);
            cts.Cancel();
        }, TestContext.Current.CancellationToken);

        await FluentActions.Awaiting(() => evt.AwaitNextAsync(filter, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();

        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(c_delayMs);
    }

    [Fact]
    public async Task AwaitNextAsync_MultipleFilters_CanBeCancelled() {
        var filter1 = filters[0];
        var filter2 = filters[1];
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);

        stopwatch.Start();

        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () => {
            await Task.Delay(c_delayMs);
            cts.Cancel();
        }, TestContext.Current.CancellationToken);

        await FluentActions.Awaiting(() => evt.AwaitNextAsync(filters, cancellationToken: cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();

        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(c_delayMs);
    }

    [Fact]
    public async Task AwaitNextAsync_ParamsFilters_CanBeCancelled() {
        var filter1 = filters[0];
        var filter2 = filters[1];
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Block);

        stopwatch.Start();
        using var cts = new CancellationTokenSource();
        _ = Task.Run(async () => {
            await Task.Delay(c_delayMs);
            cts.Cancel();
        }, TestContext.Current.CancellationToken);

        await FluentActions.Awaiting(() => evt.AwaitNextAsync(cts.Token, filter1, filter2))
            .Should().ThrowAsync<OperationCanceledException>();

        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(c_delayMs);
    }
    
    // ===== Pre-Cancelled Token ===== //
    [Fact]
    public async Task AwaitNextAsync_NoFilter_PreCancelledToken_ThrowsImmediately() {
        using var cts = new CancellationTokenSource();
        await CancelAsync(cts);
        
        stopwatch.Start();
        await FluentActions.Awaiting(() => evt.AwaitNextAsync(cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
        stopwatch.Stop();
        
        // Should throw immediately without waiting
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(c_delayMs);
    }
    
    [Fact]
    public async Task AwaitNextAsync_OneFilter_PreCancelledToken_ThrowsImmediately() {
        var filter = filters[0];
        filter.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        
        using var cts = new CancellationTokenSource();
        await CancelAsync(cts);
        
        stopwatch.Start();
        await FluentActions.Awaiting(() => evt.AwaitNextAsync(filter, cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
        stopwatch.Stop();
        
        // Should throw immediately without waiting
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(c_delayMs);
    }
    
    [Fact]
    public async Task AwaitNextAsync_MultipleFilters_PreCancelledToken_ThrowsImmediately() {
        var filter1 = filters[0];
        var filter2 = filters[1];
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        
        using var cts = new CancellationTokenSource();
        await CancelAsync(cts);
        
        stopwatch.Start();
        await FluentActions.Awaiting(() => evt.AwaitNextAsync(filters, cancellationToken: cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
        stopwatch.Stop();
        
        // Should throw immediately without waiting
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(c_delayMs);
    }
    
    [Fact]
    public async Task AwaitNextAsync_ParamsFilters_PreCancelledToken_ThrowsImmediately() {
        var filter1 = filters[0];
        var filter2 = filters[1];
        filter1.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        filter2.Filter(ref Arg.Any<int>()).Returns(FilterResult.Pass);
        
        using var cts = new CancellationTokenSource();
        await CancelAsync(cts);
        
        stopwatch.Start();
        await FluentActions.Awaiting(() => evt.AwaitNextAsync(cts.Token, filter1, filter2))
            .Should().ThrowAsync<OperationCanceledException>();
        stopwatch.Stop();
        
        // Should throw immediately without waiting
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(c_delayMs);
    }
    
    Task CancelAsync(CancellationTokenSource cts) {
#if NET7_0_OR_GREATER
        return cts.CancelAsync();
#else
        cts.Cancel();
        return Task.CompletedTask;
#endif
    }
}