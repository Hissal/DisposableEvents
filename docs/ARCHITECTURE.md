# Architecture

An orientation map for the DisposableEvents codebase — enough to know *where to go to change
something*. It is not an API reference; the API is documented by XML comments on the types
themselves, which is the copy that cannot rot.

## Packages

| Project | NuGet package | Depends on |
|---|---|---|
| `DisposableEvents/` | `DisposableEvents` | `HCommons.Disposables`, `HCommons.Void`, `HCommons.Buffers` |
| `DisposableEvents.R3/` | `DisposableEvents.R3` | core (ProjectReference) + `R3` |
| `DisposableEvents.ZLinq/` | `DisposableEvents.ZLinq` | core (ProjectReference) + `ZLinq` |

The dependency arrow only ever points *into* the core. The core knows nothing about R3 or ZLinq,
and the two integration packages know nothing about each other. All three are packable and ship
under one shared version number (see [CONTRIBUTING.md](../CONTRIBUTING.md#releases)).

The `HCommons.*` packages are external and supply `Disposable.Empty` / `DisposableBag`, the `Void`
unit type used by the parameterless overloads, and `PooledArray<T>`.

## Two parallel families

Everything in the core belongs to one of two families:

- **Events** — fire-and-forget. `Publish(TMessage)` returns nothing.
  `IEventPublisher<TMessage>` + `IEventSubscriber<TMessage>` = `IDisposableEvent<TMessage>`.
- **Funcs** — request/response. `Invoke(TArg)` returns a `FuncResult<TResult>`, a value-or-nothing
  struct. `IFuncPublisher<TArg, TResult>` + `IFuncSubscriber<TArg, TResult>` =
  `IDisposableFunc<TArg, TResult>`. Which handler's result you get back depends on the func type:
  `DisposableFunc` returns the last handler's, `FirstFunc` short-circuits on the first non-null one.
  To see *every* handler's result, use the `Invoke*` publisher extensions
  (`InvokeToArray`, `InvokeAsEnumerable`, `InvokeForEach`) in `Funcs/FuncPublisherExtensions/`.

The two are structurally mirrored, file for file:

| Events | Funcs |
|---|---|
| `Events/EventCore.cs` | `Funcs/FuncCore.cs` |
| `Events/EventHandler.cs` (`IEventHandler<T>`) | `Funcs/FuncHandler.cs` (`IFuncHandler<TArg, TResult>`) |
| `Events/AbstractSubscriber.cs` | `Funcs/AbstractFuncSubscriber.cs` |
| `Events/EventSubscriberExtensions.cs` | `Funcs/FuncSubscriberExtensions.cs` |
| `Events/VoidEventExtensions.cs` | `Funcs/VoidFuncExtensions.cs` |

**If you change one side, check the mirror.** The one intentional asymmetry is the verb: events use
`Subscribe`, funcs use `AddHandler`.

## Layers of the core package

Roughly bottom-up. Each layer only knows about the ones below it.

### 1. Storage and dispatch — `Events/EventCore.cs`, `Funcs/FuncCore.cs`

The only place handlers actually live. `FreeList<T>` holds handlers in reusable slots and hands back
a `FreeListKey`; `Subscribe` returns a `Subscription` token holding the core plus that key, and
disposing it removes the slot. All mutation happens under `lock (gate)`. A `PooledArray<T>` snapshot
of the handler list is cached and invalidated whenever handlers change.

Two behaviours worth knowing before touching this file: a key invalidated by `ClearHandlers` is
stale, so disposing that subscription must stay a no-op; and `Publish` deliberately iterates the
`FreeList` directly rather than taking the lock.

### 2. Event types — `Events/EventTypes/`

Decorators over the core, all implementing `IPipelineEvent<TMessage>` except `DisposableEvent`,
which is a plain pass-through wrapper:

| Type | Behaviour |
|---|---|
| `DisposableEvent<T>` / `DisposableEvent` | Thin wrapper over `EventCore`. The default. |
| `BufferedEvent<T>` | Replays the last published message to each new subscriber. |
| `FilteredEvent<T>` | Applies filters once, at **publish** time. |
| `FilterAttachingEvent<T>` | Attaches default filters to every **subscription**. |
| `ForwardingEvent<T>` | Fans out to other events; `ForwardFlags` / `ForwardTiming` pick what and when. |

Each decorator wraps `Internal/LazyInnerEvent.cs`, which defers creating the real `EventCore` until
first use so that a node can instead be wired to a pipeline successor.

`LightEvent.cs` is commented-out work in progress. Leave it alone.

### 3. Composition — `EventPipeline.cs`

`EventPipeline<T>.Manual()` returns an `EventPipelineBuilder<T>` that chains `IPipelineEvent<T>`
nodes via `Next(...)`. Each event type contributes its own builder step as an extension method — for
example `BufferResponse()` at the bottom of `BufferedEvent.cs`. That is where a new node type gets
its fluent entry point.

### 4. Hubs — `Hub/Event/`

Type-keyed lookup, so unrelated components can share an event without sharing a reference.

- `EventRegistry` — `ConcurrentDictionary<Type, IEventMarker>`, the storage.
- `EventHub` — creates events on demand through an `IEventFactory`.
- `ManualEventHub` — register everything up front via `CreateBuilder()`; unknown types throw.
- `GlobalEventHub` — static ambient hub; throws until `SetHub` is called.

Each has an `EventHub<TMessageRestriction>` variant that constrains which message types may pass. A
disposed hub returns `NullEvent<T>.Instance` rather than throwing.

### 5. Ergonomics — extension classes

`EventSubscriberExtensions` (`Action<T>` overloads, predicate filters, `params` filter arrays),
`VoidEventExtensions` (parameterless overloads over `Void`), `EventSubscribeOnceExtensions` +
`OneShotEventHandler`, `EventAwaitNextExtensions` + `AwaitNextAsyncEventHandler` (awaits the next
message, honours a `CancellationToken`), and `ObservableExtensions` (`AsObservable`, BCL
`IObservable<T>`).

New sugar belongs here, as an extension on `IEventSubscriber<T>` / `IEventPublisher<T>` — not as a
new member on the interfaces, which is expensive (see [Multi-targeting](#multi-targeting)).

### 6. Filters — `Filters/`

`IEventFilter<TMessage>.Filter(ref TMessage value)` returns a `FilterResult` (`Pass` / `Block`,
implicitly convertible to and from `bool`). The `ref` is the point: a filter may also *mutate* the
message — see `ValueMutatorFilter`. Ordering comes from `IEventFilter.FilterOrder` combined with a
`FilterOrdering` mode (`KeepOriginal`, `StableSort`, `UnstableSort`), and `CompositeEventFilter`
flattens many filters into one.

The same filter can be applied at three different points, which is the main thing to get right:

1. per handler, at subscribe time — wrapped into a `FilteredEventHandler` by the factory;
2. once per publish — `FilteredEvent`;
3. attached to every subscription by the event itself — `FilterAttachingEvent`.

### 7. Configuration — `Configuration.cs`, `Factories/`

`DisposableEvents.Configure(cfg => ...)` replaces the process-wide `DisposableEventsConfig`
(`InitialSubscriberCapacity` plus the factories); `GlobalConfig` is the internal read facade.
`IEventFactory`, `IFilteredEventHandlerFactory` and `IFilteredFuncHandlerFactory` are the
substitution points — change what gets allocated here rather than inside the event types.

### 8. Internals — `Internal/`

`FreeList`, `FastQueue`, `ArrayOrOne` (holds either one item or an array, avoiding an allocation in
the common single-item case), `Optional`, `NullEvent`, `LazyInnerEvent`, and
`Polyfill/RuntimeHelpers`. `InternalsVisibleTo.cs` opens these to `DisposableEvents.Tests` and
`Benchmarks` only.

## Integration packages

**R3** — `AsR3Observable()` on any `IEventSubscriber<T>`, backed by `R3ObservableAdapter<T>`. Note
that the csproj `<Compile Include>`-links `Internal/ArrayOrOne.cs` from the core: internal types are
reused by source rather than made public.

**ZLinq** — turns an `IFuncPublisher` into a `ValueEnumerable`, in two flavours:
`InvokeAsValueEnumerable` (deferred — handlers run as you iterate, and stopping early skips the
rest) and `InvokeAsValueEnumerableImmediate` (eager). It also mirrors the core `FuncResult`
extensions — `Combine`, `FirstOrDefault`, `ForEach`, `GetValues`, `Match`, `Switch` — in ZLinq terms.

Both packages are adapters only. Behaviour changes belong in the core.

## Multi-targeting

Libraries target `net9.0;net8.0;netstandard2.0;netstandard2.1`. The constraint that shapes the code
is that **netstandard2.0 has no default interface methods**. So:

- `IEventSubscriber<T>` and `IFuncSubscriber<TArg, TResult>` wrap their filter overloads in
  `#if NETSTANDARD2_0` (declaration only) `#else` (default implementation).
- `AbstractSubscriber<T>` and `AbstractFuncSubscriber<TArg, TResult>` exist purely to supply those
  same overloads as a base class for the netstandard2.0 build. Every concrete event and func type
  derives from one of them.
- `EventSubscriberExtensions` re-exposes the same overloads once more as extension methods, so
  callers get one shape across every target.

**Adding a member to a public interface therefore means editing three places, not one.** Prefer an
extension method.

Other target-specific pieces: PolySharp on both netstandard targets, `System.Buffers` and
`System.Memory` on 2.0 only, an `#if NETSTANDARD2_0` branch in `EventRegistry` covering the missing
`ConcurrentDictionary.GetOrAdd` overloads, and `IsTrimmable` on the modern targets only.

## Tests and sandbox

`DisposableEvents.Tests/` mirrors the source tree directory for directory. xunit.v3 on
Microsoft.Testing.Platform, targeting `net6.0;net8.0;net9.0` plus `net48` on Windows. Conventions
live in [TESTING.md](TESTING.md).

The `sandbox` solution folder is not packable and not part of the public contract: `Benchmarks`
(BenchmarkDotNet, compared against MessagePipe and R3), plus `InternalsInvisible` and
`TestPlayGround`, which are currently empty scratch projects.

## Where do I go to change X

| Change | Start here |
|---|---|
| How handlers are stored, dispatched, or unsubscribed | `Events/EventCore.cs`, `Funcs/FuncCore.cs` |
| A new kind of event behaviour | new file in `Events/EventTypes/`, implementing `IPipelineEvent<T>` |
| A new fluent pipeline step | the extension class at the bottom of that event type's file |
| Subscribe / publish convenience overloads | `Events/EventSubscriberExtensions.cs` and its `Void` mirror |
| Filter semantics or ordering | `Filters/` |
| What gets allocated on subscribe | `Factories/`, then `Configuration.cs` |
| Type-keyed event lookup | `Hub/Event/` |
| R3 or ZLinq surface | the matching integration project — never the core |
| A netstandard2.0 break | the `#if NETSTANDARD2_0` blocks, plus `AbstractSubscriber` |
| Package metadata or target frameworks | the individual `.csproj` — there is no `Directory.Build.props` |
