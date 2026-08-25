# DisposableEvents

[![Test and Build .NET](https://github.com/Hissal/DisposableEvents/actions/workflows/build-debug.yml/badge.svg)](https://github.com/Hissal/DisposableEvents/actions/workflows/build-debug.yml)

PubSub style events that return an `IDisposable` subscription. Subscribe, keep the handle, dispose
it to unsubscribe — no `-=`, no forgotten handler leaks.

> Pre-release. The public API may still change between `1.0.0-betaN` versions.

## Install

```bash
dotnet add package DisposableEvents
```

| Package | What it adds |
|---|---|
| [`DisposableEvents`](https://www.nuget.org/packages/DisposableEvents) | The library. No dependencies beyond `HCommons.*`. |
| [`DisposableEvents.R3`](https://www.nuget.org/packages/DisposableEvents.R3) | Expose any event as an [R3](https://github.com/Cysharp/R3) `Observable<T>` |
| [`DisposableEvents.ZLinq`](https://www.nuget.org/packages/DisposableEvents.ZLinq) | Zero-allocation [ZLinq](https://github.com/Cysharp/ZLinq) queries over func results |

Targets `net9.0`, `net8.0`, `netstandard2.1`, and `netstandard2.0`.

## Quickstart

```csharp
using DisposableEvents;

var clicked = new DisposableEvent<string>();

IDisposable subscription = clicked.Subscribe(name => Console.WriteLine($"clicked: {name}"));

clicked.Publish("save");    // clicked: save

subscription.Dispose();
clicked.Publish("quit");    // nothing — the handler is gone
```

Filter at the subscription, so a handler only sees what it cares about:

```csharp
var score = new DisposableEvent<int>();

score.Subscribe(n => Console.WriteLine($"high score: {n}"), n => n > 9000);

score.Publish(10);      // ignored
score.Publish(9001);    // high score: 9001
```

Events with no payload need no payload type:

```csharp
var saved = new DisposableEvent();

saved.Subscribe(() => Console.WriteLine("saved"));
saved.Publish();
```

## Beyond the basics

**A hub**, so two components share an event without sharing a reference — the message type is the
key:

```csharp
var hub = new EventHub();

hub.GetSubscriber<PlayerDied>().Subscribe(e => ShowGameOver(e.Cause));
hub.GetPublisher<PlayerDied>().Publish(new PlayerDied("fell in a hole"));
```

**Funcs**, when you want an answer back rather than fire-and-forget:

```csharp
var canSave = new DisposableFunc<string, bool>();

canSave.AddHandler(path => path.EndsWith(".json"));

FuncResult<bool> result = canSave.Invoke("world.json");
if (result.TryGetValue(out var allowed) && allowed) { /* ... */ }
```

**Event types** you can compose into a pipeline — `BufferedEvent` replays the last message to new
subscribers, `FilteredEvent` filters once at publish time, `FilterAttachingEvent` applies default
filters to every subscription, `ForwardingEvent` fans out to other events.

**Await the next message**, with cancellation support:

```csharp
var message = await chat.AwaitNextAsync(cancellationToken);
```

**One-shot subscriptions**, which unsubscribe themselves after the first message:

```csharp
ready.SubscribeOnce(() => Start());
```

## Documentation

Links are absolute so they also work from the rendered README on nuget.org.

- [Architecture](https://github.com/Hissal/DisposableEvents/blob/main/docs/ARCHITECTURE.md) — how the codebase fits together
- [Contributing](https://github.com/Hissal/DisposableEvents/blob/main/CONTRIBUTING.md) — PRs, labels, building, releases
- [AGENTS.md](https://github.com/Hissal/DisposableEvents/blob/main/AGENTS.md) — guidance for AI coding agents
- [Releases](https://github.com/Hissal/DisposableEvents/releases) — changelog per version

## License

[MIT](https://github.com/Hissal/DisposableEvents/blob/main/LICENSE) © Lassi Harjaluoma
