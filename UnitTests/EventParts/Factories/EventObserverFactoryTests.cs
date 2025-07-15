using DisposableEvents;
using DisposableEvents.Factories;

namespace UnitTests.EventParts.Factories;

[TestFixture]
public class EventObserverFactoryTests {
    [Test]
    public void Create_WithNoFilters_ReturnsOriginalObserver() {
        var factory = new EventObserverFactory();
        var observer = new TestObserver<int>();
        
        var result = factory.Create(observer, Array.Empty<IEventFilter<int>>());
        
        Assert.That(result, Is.SameAs(observer));
    }

    [Test]
    public void Create_WithOneFilter_AppliesGivenFilterToObserver() {
        var factory = new EventObserverFactory();
        var observer = new TestObserver<int>();
        var filter = new TestFilter<int>();
        
        var result = factory.Create(observer, new IEventFilter<int>[] { filter });
        result.OnNext(10);
        
        Assert.That(filter.LastValue, Is.EqualTo(10));
    }


    [Test]
    public void Create_WithMultipleFilters_AppliesGivenFiltersToObserver() {
        var factory = new EventObserverFactory();
        var observer = new TestObserver<int>();
        var filter1 = new TestFilter<int>();
        var filter2 = new TestFilter<int>();
        
        var result = factory.Create(observer, new IEventFilter<int>[] { filter1, filter2 });
        result.OnNext(20);
        
        Assert.Multiple(() => {
            Assert.That(filter1.LastValue, Is.EqualTo(20));
            Assert.That(filter2.LastValue, Is.EqualTo(20));
        });
    }
    
    [Test]
    public void EventObserverFactory_Default_ReturnsSameInstance() {
        var factory1 = EventObserverFactory.Default;
        var factory2 = EventObserverFactory.Default;
        
        Assert.That(factory1, Is.SameAs(factory2), "Default factory should be a singleton instance.");
    }
}