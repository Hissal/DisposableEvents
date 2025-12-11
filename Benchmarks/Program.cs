// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using DisposableEvents;
using DisposableEvents.Internal;
using DisposableEvents.R3;
using DisposableEvents.ZLinq;
using MessagePipe;
using Microsoft.Extensions.DependencyInjection;
using Perfolizer.Horology;
using Perfolizer.Mathematics.OutlierDetection;
using R3;
using ZLinq;


BenchmarkRunner.Run<EventBenchMarks>(new AccurateConfig());

public sealed class AccurateConfig : ManualConfig {
    public AccurateConfig() {
        var accurateJob = Job.Default
            .WithRuntime(CoreRuntime.Core90) // Pin runtime
            .WithPlatform(Platform.X64)
            .WithJit(Jit.RyuJit)
            .WithId("Accurate")
            .WithLaunchCount(2) // Multiple processes to average OS noise
            .WithWarmupCount(10) // Longer warmup to reach steady state
            .WithIterationTime(TimeInterval.FromMilliseconds(250))
            .WithMinIterationCount(25) // Enough samples
            .WithMaxIterationCount(50)
            .WithUnrollFactor(1) // Avoid unrolling distortion
            .WithStrategy(RunStrategy.Throughput)
            .WithGcServer(true) // Throughput-friendly and consistent GC
            .WithGcConcurrent(false) // Reduce background GC noise
            .WithGcForce(true) // Force full GC between iterations
            .WithAffinity(new IntPtr(1)) // Pin to a single CPU core (core 0)
            .WithEnvironmentVariables( // Stabilize JIT behavior
                new EnvironmentVariable("COMPlus_ReadyToRun", "0"),
                new EnvironmentVariable("COMPlus_TieredCompilation", "0"));

        AddJob(accurateJob);

        // Add at least one logger to see progress
        AddLogger(ConsoleLogger.Default);

        AddDiagnoser(MemoryDiagnoser.Default);
        // Optional if you want to spot thread switching/contention (adds overhead):
        // AddDiagnoser(new ThreadingDiagnoser());

        AddColumnProvider(DefaultColumnProviders.Instance);
        AddExporter(MarkdownExporter.GitHub);

        WithOptions(ConfigOptions.KeepBenchmarkFiles); // Keep artifacts for inspection
        Orderer = new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest);
    }
}

[MemoryDiagnoser]
[Outliers(OutlierMode.RemoveAll)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class FuncBenchMarks {
    const string c_creation = "1Creation";
    const string c_pubSub = "2PubSub";
    const string c_publish = "2Publish";
    const string c_enumerations = "3Enumerations";
    
    const int c_iterationCount = 1;
    const int c_subCount = 2;
    const int c_unsubCount = 1;
    const int c_pubCount = 1;
    
    const int c_pubOnlyPubCount = 1;
    const int c_pubOnlySubCount = 10;
    
    const int c_enumerationPubCount = 1;
    
    FirstFunc<int, int> firstFunc;
    DisposableFunc<int, int> func;
    DisposableEvent<int> de;
    
    IDisposable?[] firstFuncSubs;
    IDisposable?[] funcSubs;
    FuncResult<int>[] funcResults;
    
    IDisposable?[] deSubs;
    
    DisposableFunc<int, int> funcPubOnly;
    DisposableEvent<int> dePubOnly;
    int pubOnlyEventResult;

    static int Max(params int[] args) {
        int high = int.MinValue;
        foreach (var v in args) {
            if (v > high)
                high = v;
        }
        return high;
    }
    
    [GlobalSetup]
    public void IterationSetup() {
        firstFunc = new FirstFunc<int, int>();
        func = new DisposableFunc<int, int>();
        funcSubs = new IDisposable?[c_subCount];
        firstFuncSubs = new IDisposable?[c_subCount];
        funcResults = new FuncResult<int>[Max(c_pubCount, c_pubOnlyPubCount, c_enumerationPubCount, c_pubOnlySubCount)];
        
        de = new DisposableEvent<int>(c_subCount);
        deSubs = new IDisposable?[c_subCount];
        
        funcPubOnly = new DisposableFunc<int, int>(c_pubOnlySubCount);
        dePubOnly = new DisposableEvent<int>(c_pubOnlySubCount);
        for (int i = 0; i < c_pubOnlySubCount; i++) {
            funcPubOnly.RegisterHandler(x => x * 2);
            dePubOnly.Subscribe(this, (bench, x) => {
                bench.pubOnlyEventResult = x * 2;
            });
        }
        
        // eSubs = new IDisposable[10];
        // rpSubs = new IDisposable[10];
        // e = new DisposableEvent<int>();
        // rp = new ReactiveProperty<int>();
    }

    [GlobalCleanup]
    public void IterationCleanup() {
        firstFunc.Dispose();
        func.Dispose();
        de.Dispose();
        
        funcPubOnly.Dispose();
        dePubOnly.Dispose();
        pubOnlyEventResult = 0;
    }

    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void FuncCreation() {
    //     var f = new DisposableFunc<int, int>();
    //     f.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void EventCreation() {
    //     var e = new DisposableEvent<int>();
    //     e.Dispose();
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void EventCreation1() {
    //     var e = new DisposableEvent<int>(1);
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void EventCreation2() {
    //     var e = new DisposableEvent<int>(2);
    //     e.Dispose();
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void LightEventCreation() {
    //     var e = new LightEvent<int>();
    //     e.Dispose();
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void LightEventCreation1() {
    //     var e = new LightEvent<int>(1);
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void LightEventCreation2() {
    //     var e = new LightEvent<int>(2);
    //     e.Dispose();
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void ReactivePropertyCreation() {
    //     var rp = new ReactiveProperty<int>();
    //     rp.Dispose();
    // }
    //
    // IDisposable[] eSubs;
    // IDisposable[] rpSubs;
    // DisposableEvent<int> e;
    // ReactiveProperty<int> rp;
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void EventSubx10() {
    //     for (int i = 0; i < 10; i++) {
    //         eSubs[i]?.Dispose();
    //         eSubs[i] = e.Subscribe(x => {
    //             var y = x;
    //         });
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void EventSubAsObservablex10() {
    //     for (int i = 0; i < 10; i++) {
    //         eSubs[i]?.Dispose();
    //         eSubs[i] = e.AsR3Observable().Subscribe(x => {
    //             var y = x;
    //         });
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void ReactivePropertySubx10() {
    //     for (int i = 0; i < 10; i++) {
    //         rpSubs[i]?.Dispose();
    //         rpSubs[i] = rp.Subscribe(x => {
    //             var y = x;
    //         });
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void EventPubSubx10() {
    //     for (int i = 0; i < 10; i++) {
    //         eSubs[i]?.Dispose();
    //         eSubs[i] = e.Subscribe(x => {
    //             var y = x;
    //         });
    //     }
    //
    //     for (int i = 0; i < 10; i++) {
    //         e.Publish(i);
    //     }
    // }
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void ReactivePropertyPubSubx10() {
    //     for (int i = 0; i < 10; i++) {
    //         rpSubs[i]?.Dispose();
    //         rpSubs[i] = rp.Subscribe(x => {
    //             var y = x;
    //         });
    //     }
    //     
    //     for (int i = 0; i < 10; i++) {
    //         rp.Value = i;
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void EventPubSubAsObservablex10() {
    //     for (int i = 0; i < 10; i++) {
    //         eSubs[i]?.Dispose();
    //         eSubs[i] = e.AsR3Observable().Subscribe(x => {
    //             var y = x;
    //         });
    //     }
    //     
    //     for (int i = 0; i < 10; i++) {
    //         e.Publish(i);
    //     }
    // }
    
    //
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void FuncPubSub() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         // Subscribe
    //         for (int i = 0; i < c_subCount; i++) {
    //             funcSubs[i]?.Dispose();
    //             funcSubs[i] = func.Subscribe(x => x * 2);
    //         }
    //
    //         // Unsubscribe
    //         for (int i = 0; i < c_unsubCount; i++) {
    //             funcSubs[i]?.Dispose();
    //         }
    //
    //         // Publish
    //         for (int i = 0; i < c_pubCount; i++) {
    //             funcResults[i] = func.Publish(i);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void FirstFuncPubSub() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         // Subscribe
    //         for (int i = 0; i < c_subCount; i++) {
    //             firstFuncSubs[i]?.Dispose();
    //             firstFuncSubs[i] = firstFunc.Subscribe(x => x * 2);
    //         }
    //
    //         // Unsubscribe
    //         for (int i = 0; i < c_unsubCount; i++) {
    //             firstFuncSubs[i]?.Dispose();
    //         }
    //
    //         // Publish
    //         for (int i = 0; i < c_pubCount; i++) {
    //             funcResults[i] = firstFunc.Publish(i);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void EventPubSub() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         // Subscribe
    //         for (int i = 0; i < c_subCount; i++) {
    //             deSubs[i]?.Dispose();
    //             deSubs[i] = de.Subscribe(x => {
    //                 var y = x;
    //             });
    //         }
    //
    //         // Unsubscribe
    //         for (int i = 0; i < c_unsubCount; i++) {
    //             deSubs[i]?.Dispose();
    //         }
    //
    //         // Publish
    //         for (int i = 0; i < c_pubCount; i++) {
    //             de.Publish(i);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_publish)]
    // public void FuncPublish() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         // Publish
    //         for (int i = 0; i < c_pubOnlyPubCount; i++) {
    //             funcResults[i] = funcPubOnly.Publish(i);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_publish)]
    // public void EventPublish() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         // Publish
    //         for (int i = 0; i < c_pubOnlyPubCount; i++) {
    //             dePubOnly.Publish(i);
    //         }
    //     }
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_enumerations)]
    // public void PublishAsEnumerable() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         for (int i = 0; i < c_enumerationPubCount; i++) {
    //             funcResults[i] = funcPubOnly.PublishAsEnumerable(i)
    //                 .Where(result => result.HasValue)
    //                 .Combine((r1, r2) => {
    //                     if (r1.TryGetValue(out var v1) && r2.TryGetValue(out var v2))
    //                         return v1 + v2;
    //                     return r1 - 5;
    //                 }).Select(x => FuncResult<int>.From(x * 3))
    //                 .GetValueOrDefault(0);
    //         }
    //     }
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_enumerations)]
    // public void PublishAsEnumerableImmediate() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         for (int i = 0; i < c_enumerationPubCount; i++) {
    //             funcResults[i] = funcPubOnly.PublishAsEnumerableImmediate(i)
    //                 .Where(result => result.HasValue)
    //                 .Combine((r1, r2) => {
    //                     if (r1.TryGetValue(out var v1) && r2.TryGetValue(out var v2))
    //                         return v1 + v2;
    //                     return r1 - 5;
    //                 }).Select(x => FuncResult<int>.From(x * 3))
    //                 .GetValueOrDefault(0);
    //         }
    //     }
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_enumerations)]
    // public void PublishAsValueEnumerable() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         for (int i = 0; i < c_enumerationPubCount; i++) {
    //             funcResults[i] = funcPubOnly.PublishAsValueEnumerable(i)
    //                 .Where(result => result.HasValue)
    //                 .Combine((r1, r2) => {
    //                     if (r1.TryGetValue(out var v1) && r2.TryGetValue(out var v2))
    //                         return v1 + v2;
    //                     return r1 - 5;
    //                 }).Select(x => FuncResult<int>.From(x * 3))
    //                 .GetValueOrDefault(0);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_enumerations)]
    // public void PublishAsValueEnumerableImmediate() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         for (int i = 0; i < c_enumerationPubCount; i++) {
    //             funcResults[i] = funcPubOnly.PublishAsValueEnumerableImmediate(i)
    //                 .Where(result => result.HasValue)
    //                 .Combine((r1, r2) => {
    //                     if (r1.TryGetValue(out var v1) && r2.TryGetValue(out var v2))
    //                         return v1 + v2;
    //                     return r1 - 5;
    //                 }).Select(x => FuncResult<int>.From(x * 3))
    //                 .GetValueOrDefault(0);
    //         }
    //     }
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_enumerations)]
    // public void PublishToArray() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         for (int i = 0; i < c_enumerationPubCount; i++) {
    //             funcResults[i] = funcPubOnly.PublishToArray(i)
    //                 .Where(result => result.HasValue)
    //                 .Combine((r1, r2) => {
    //                     if (r1.TryGetValue(out var v1) && r2.TryGetValue(out var v2))
    //                         return v1 + v2;
    //                     return r1 - 5;
    //                 }).Select(x => FuncResult<int>.From(x * 3))
    //                 .GetValueOrDefault(0);
    //         }
    //     }
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_enumerations)]
    // public void PublishAsEnumerableForEach() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         for (int i = 0; i < c_enumerationPubCount; i++) {
    //             funcPubOnly.PublishAsEnumerable(i).ForEach(funcResults, (results, result, index) => {
    //                 results[index] = result;
    //             });
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_enumerations)]
    // public void PublishForEach() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         for (int i = 0; i < c_enumerationPubCount; i++) {
    //             funcPubOnly.PublishForEach(i, funcResults, (results, result, index) => {
    //                 results[index] = result;
    //             });
    //         }
    //     }
    // }
}

[MemoryDiagnoser]
[Outliers(OutlierMode.RemoveAll)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class EventBenchMarks {
    const string c_creation = "1Creation";
    const string c_pubSub = "2PubSub";
    const string c_filteredSingle = "3Single Filter";
    const string c_filteredMultiple = "4Multiple Filters";

    const int c_iterationCount = 1;
    const int c_subCountMax = 4;
    const int c_subCountMin = 4;
    const int c_unsubCountMax = 2;
    const int c_unsubCountMin = 2;
    const int c_pubCountMax = 8;
    const int c_pubCountMin = 8;
    const int c_filterCount = 10;

    (int subCount, int unsubCount, int pubCount, int filterCount) GetCounts(int k) {
        var subCount = c_subCountMax <= c_subCountMin ? c_subCountMax
            : c_subCountMin + (k % (c_subCountMax - c_subCountMin));
        var unsubCount = c_unsubCountMax <= c_unsubCountMin ? c_unsubCountMax
            : c_unsubCountMin + (k % (c_unsubCountMax - c_unsubCountMin));
        var pubCount = c_pubCountMax <= c_pubCountMin ? c_pubCountMax
            : c_pubCountMin + (k % (c_pubCountMax - c_pubCountMin));
        var filterCount = c_filterCount;
        return (subCount, unsubCount, pubCount, filterCount);
    }
    
    DisposableEvent<int> de;
    MPEvent<int> mpe;
    event Action<int> ae = delegate { };

    IDisposable[] deSubs;
    IDisposable[] mpeSubs;
    Action<int>[] actions;

    ServiceProvider provider;

    EventHub hub1;
    EventHub hub2;
    IEventPublisher<int> pub1;
    
    [GlobalSetup]
    public void GlobalSetup() {
        var services = new ServiceCollection();
        services.AddMessagePipe();
        provider = services.BuildServiceProvider();
        GlobalMessagePipe.SetProvider(provider);
        GlobalEventHub.SetHub(new EventHub());
    }

    [GlobalCleanup]
    public void GlobalCleanup() {
        provider.Dispose();
    }

    [IterationSetup]
    public void IterationSetup() {
        de = new DisposableEvent<int>(4);
        mpe = new MPEvent<int>();

        deSubs = new IDisposable[c_subCountMax];
        mpeSubs = new IDisposable[c_subCountMax];
        actions = new Action<int>[c_subCountMax];
        
        hub1 = new EventHub();
        hub2 = new EventHub();
        hub1.GetSubscriber<int>().Subscribe(x => { var y = x; });
        hub2.GetSubscriber<int>().Subscribe(x => { var y = x; });
        
        pub1 = hub1.GetPublisher<int>();
    }

    [IterationCleanup]
    public void IterationCleanup() {
        de.Dispose();
        mpe.Dispose();
    }

    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void DisposableEventInstantCoreCreation() {
    //     var e = new DisposableEventInstantCore<int>(4);
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void DisposableEventLazyCoreCreation() {
    //     var e = new DisposableEventLazyCore<int>(4);
    //     e.Dispose();
    // }

    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void EventHubCreation() {
    //     var hub = new EventHub();
    //     hub.Dispose();
    // }
    
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void EventHubGet() {
    //     var pub = GlobalEventHub.GetPublisher<int>();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void MessagePipeGet() {
    //     var pub = GlobalMessagePipe.GetPublisher<int>();
    // }

    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void DisposableEventPipelineFilter() {
    //     var e = EventPipeline<int>.Manual()
    //         .BufferResponse()
    //         .Filter(x => x % 2 == 0)
    //         .BufferResponse()
    //         .Build();
    //     
    //     var sub = e.Subscribe(i => { var x = i; });
    //     e.Publish(1);
    //     sub.Dispose();
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void DisposableEventPipelineAttachFilter() {
    //     var e = EventPipeline<int>.Manual()
    //         .BufferResponse()
    //         .AttachFilter(x => x % 2 == 0)
    //         .BufferResponse()
    //         .Build();
    //     
    //     var sub = e.Subscribe(i => { var x = i; });
    //     e.Publish(1);
    //     sub.Dispose();
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void R3Observable() {
    //     var e = new DisposableEvent<int>();
    //     var obs = e.AsObservable().ToObservable();
    //     var sub = obs.Subscribe(i => { var x = i; });
    //     e.Publish(1);
    //     sub.Dispose();
    //     e.Dispose();
    // }

    
    // [Benchmark]
    // [BenchmarkCategory(c_creation)]
    // public void MessagePipeEventCreation() {
    //     var e = new MPEvent<int>();
    //     e.Dispose();
    // }

    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void DisposableEventFullPipeline() {
    //     var e = new DisposableEvent<int>(4);
    //     var sub = e.Subscribe(i => { var x = i; });
    //     e.Publish(1);
    //     sub.Dispose();
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void DisposableEventLazyCoreFullPipeline() {
    //     var e = new DisposableEventLazyCore<int>(4);
    //     var sub = e.Subscribe(i => { var x = i; });
    //     e.Publish(1);
    //     sub.Dispose();
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void MessagePipeEventFullPipeline() {
    //     var e = new MPEvent<int>();
    //     var sub = e.Subscribe(i => { var x = i; });
    //     e.Publish(1);
    //     sub.Dispose();
    //     e.Dispose();
    // }
    //
    //
    // [Benchmark]
    // [BenchmarkCategory("pub")]
    // public void DisposableEventPublish() {
    //     var e = new DisposableEvent<int>(4);
    //     e.Publish(1);
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory("pub")]
    // public void DisposableEventLazyCorePublish() {
    //     var e = new DisposableEventLazyCore<int>(4);
    //     e.Publish(1);
    //     e.Dispose();
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory("pub")]
    // public void MessagePipeEventPublish() {
    //     var e = new MPEvent<int>();
    //     e.Publish(1);
    //     e.Dispose();
    // }
    
    
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void DisposableEventPubSub() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         var (subCount, unsubCount, pubCount, filterCount) = GetCounts(k);
    //
    //         for (int i = 0; i < subCount; i++) {
    //             deSubs[i]?.Dispose();
    //             deSubs[i] = de.Subscribe(j => {
    //                 var x = j;
    //             });
    //         }
    //
    //         for (int i = 0; i < unsubCount; i++) {
    //             deSubs[i]?.Dispose();
    //         }
    //
    //         for (int i = 0; i < pubCount; i++) {
    //             de.Publish(i);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void MessagePipeEventPubSub() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         var (subCount, unsubCount, pubCount, filterCount) = GetCounts(k);
    //
    //         for (int i = 0; i < subCount; i++) {
    //             mpeSubs[i]?.Dispose();
    //             mpeSubs[i] = mpe.Subscribe(j => {
    //                 var x = j;
    //             });
    //         }
    //
    //         for (int i = 0; i < unsubCount; i++) {
    //             mpeSubs[i]?.Dispose();
    //         }
    //
    //         for (int i = 0; i < pubCount; i++) {
    //             mpe.Publish(i);
    //         }
    //     }
    // }
    
    
    //
    // [Benchmark]
    // [BenchmarkCategory(c_pubSub)]
    // public void ActionPubSub() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         var (subCount, unsubCount, pubCount, filterCount) = GetCounts(k);
    //
    //         // Subscribe
    //         for (int i = 0; i < subCount; i++) {
    //             if (actions[i] != null)
    //                 ae -= actions[i];
    //             
    //             actions[i] = j => { var x = j; };
    //             ae += actions[i];
    //         }
    //
    //         // Unsubscribe
    //         for (int i = 0; i < unsubCount; i++) {
    //             if (actions[i] != null) {
    //                 ae -= actions[i];
    //                 actions[i] = null;
    //             }
    //         }
    //
    //         // Publish
    //         for (int i = 0; i < pubCount; i++) {
    //             ae?.Invoke(i);
    //         }
    //     }
    // }
    
    //
    // [Benchmark]
    // [BenchmarkCategory(c_filteredSingle)]
    // public void DisposableEventPubSubFilteredSingle() {
    //     var filter = new PredicateEventFilter<int>(i => i % 2 == 0);
    //
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         var (subCount, unsubCount, pubCount, filterCount) = GetCounts(k);
    //
    //         for (int i = 0; i < subCount; i++) {
    //             deSubs[i]?.Dispose();
    //             deSubs[i] = de.Subscribe((AnonymousEventHandler<int>)(j => {
    //                 var x = j;
    //             }), filter);
    //         }
    //
    //         for (int i = 0; i < unsubCount; i++) {
    //             deSubs[i]?.Dispose();
    //         }
    //
    //         for (int i = 0; i < pubCount; i++) {
    //             de.Publish(i);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_filteredSingle)]
    // public void MessagePipeEventPubSubFilteredSingle() {
    //     var filter = new PredicateFilter<int>(i => i % 2 == 0);
    //
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         var (subCount, unsubCount, pubCount, filterCount) = GetCounts(k);
    //
    //         for (int i = 0; i < subCount; i++) {
    //             mpeSubs[i]?.Dispose();
    //             mpeSubs[i] = mpe.Subscribe(j => {
    //                 var x = j;
    //             }, filter);
    //         }
    //
    //         for (int i = 0; i < unsubCount; i++) {
    //             mpeSubs[i]?.Dispose();
    //         }
    //
    //         for (int i = 0; i < pubCount; i++) {
    //             mpe.Publish(i);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_filteredMultiple)]
    // public void DisposableEventPubSubFilteredMultiple() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         var (subCount, unsubCount, pubCount, filterCount) = GetCounts(k);
    //
    //         var filters = new IEventFilter<int>[filterCount];
    //         for (int i = 0; i < filterCount; i++) {
    //             filters[i] = new PredicateEventFilter<int>(j => j >= 1);
    //         }
    //         
    //         for (int i = 0; i < subCount; i++) {
    //             deSubs[i]?.Dispose();
    //             deSubs[i] = de.Subscribe((AnonymousEventHandler<int>)(j => {
    //                 var x = j;
    //             }), filters);
    //         }
    //
    //         for (int i = 0; i < unsubCount; i++) {
    //             deSubs[i]?.Dispose();
    //         }
    //
    //         for (int i = 0; i < pubCount; i++) {
    //             de.Publish(i);
    //         }
    //     }
    // }
    //
    // [Benchmark]
    // [BenchmarkCategory(c_filteredMultiple)]
    // public void MessagePipeEventPubSubFilteredMultiple() {
    //     for (int k = 0; k < c_iterationCount; k++) {
    //         var (subCount, unsubCount, pubCount, filterCount) = GetCounts(k);
    //
    //         var filters = new MessageHandlerFilter<int>[filterCount];
    //         for (int i = 0; i < filterCount; i++) {
    //             filters[i] = new PredicateFilter<int>(j => j >= 1);
    //         }
    //         
    //         for (int i = 0; i < subCount; i++) {
    //             mpeSubs[i]?.Dispose();
    //             mpeSubs[i] = mpe.Subscribe(j => {
    //                 var x = j;
    //             }, filters);
    //         }
    //
    //         for (int i = 0; i < unsubCount; i++) {
    //             mpeSubs[i]?.Dispose();
    //         }
    //
    //         for (int i = 0; i < pubCount; i++) {
    //             mpe.Publish(i);
    //         }
    //     }
    // }
}

internal sealed class PredicateFilter<T> : MessageHandlerFilter<T> {
    private readonly System.Func<T, bool> predicate;

    public PredicateFilter(System.Func<T, bool> predicate) {
        this.predicate = predicate;
        this.Order = int.MinValue;
    }

    public override void Handle(T message, Action<T> next) {
        if (!this.predicate(message))
            return;
        next(message);
    }
}

public record MPEvent<T> : IDisposable {
    IDisposablePublisher<T>? publisher;
    ISubscriber<T>? subscriber;

    public IDisposablePublisher<T> GetPublisher() {
        if (publisher == null) {
            CreateEvent();
        }

        return publisher!;
    }

    public ISubscriber<T> GetSubscriber() {
        if (subscriber == null) {
            CreateEvent();
        }

        return subscriber!;
    }

    EventFactory? factory;

    public MPEvent(EventFactory eventFactory = null) {
        factory = eventFactory;

        try {
            CreateEvent();
        }
        catch {
            // noop
        }
    }

    void CreateEvent() {
        if (factory == null) {
            (publisher, subscriber) = GlobalMessagePipe.CreateEvent<T>();
        }
        else {
            (publisher, subscriber) = factory.CreateEvent<T>();
            factory = null;
        }
    }

    public void Publish(T message) =>
        GetPublisher().Publish(message);

    public IDisposable Subscribe(IMessageHandler<T> handler, params MessageHandlerFilter<T>[] filters) =>
        GetSubscriber().Subscribe(handler, filters);

    public IDisposable Subscribe(Action<T> handler, params MessageHandlerFilter<T>[] filters) =>
        GetSubscriber().Subscribe(handler, filters);

    public IDisposable Subscribe(Action<T> handler, System.Func<T, bool> predicate,
        params MessageHandlerFilter<T>[] filters) =>
        GetSubscriber().Subscribe(handler, predicate, filters);

    public void Dispose() {
        publisher?.Dispose();
    }
}

[MemoryDiagnoser]
[Outliers(OutlierMode.RemoveAll)]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ArrayOrOneVsArray {
    const string c_one = "One";
    const string c_many = "Many";
    
    class OBJ {
        public int Value;
        public OBJ(int v) { Value = v; }
        
        public static implicit operator OBJ(int v) => new OBJ(v);
    }
    
    [Benchmark]
    [BenchmarkCategory(c_one)]
    public void ArrayOrOne_One() {
        var arrOrOne = new ArrayOrOne<OBJ>(1);
    }
    [Benchmark]
    [BenchmarkCategory(c_one)]
    public void Array_One() {
        var arr = new OBJ[1] { 1 };
    }
    [Benchmark]
    [BenchmarkCategory(c_many)]
    public void ArrayOrOne_Many() {
        var arrOrOne = new ArrayOrOne<OBJ>(new OBJ[] { 1, 2, 3, 4, 5 });
    }
    [Benchmark]
    [BenchmarkCategory(c_many)]
    public void Array_Many() {
        var arr = new OBJ[5] { 1, 2, 3, 4, 5 };
    }
}