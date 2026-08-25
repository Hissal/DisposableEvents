# DisposableEvents.R3

[R3](https://github.com/Cysharp/R3) integration for
[DisposableEvents](https://github.com/Hissal/DisposableEvents). Turns any DisposableEvents
subscriber into an R3 `Observable<T>`, so events can be composed with R3 operators.

## Install

```bash
dotnet add package DisposableEvents.R3
```

Brings `DisposableEvents` and `R3` with it. Targets `net9.0`, `net8.0`, `netstandard2.1`, and
`netstandard2.0`.

## Usage

```csharp
using DisposableEvents;
using DisposableEvents.R3;
using R3;

var score = new DisposableEvent<int>();

using var subscription = score.AsR3Observable()
    .Where(n => n > 9000)
    .Distinct()
    .Subscribe(n => Console.WriteLine($"high score: {n}"));

score.Publish(10);      // filtered out
score.Publish(9001);    // high score: 9001
```

`AsR3Observable` works on any `IEventSubscriber<T>` — a plain event, a pipeline, or a subscriber
handed out by an `EventHub`. It also takes DisposableEvents filters directly, applied before the
message reaches the observable:

```csharp
var observable = score.AsR3Observable(new PredicateEventFilter<int>(n => n > 0));
```

Disposing the R3 subscription unsubscribes from the underlying event.

## Documentation

[Main repository and README](https://github.com/Hissal/DisposableEvents) ·
[Architecture](https://github.com/Hissal/DisposableEvents/blob/main/docs/ARCHITECTURE.md) ·
[Releases](https://github.com/Hissal/DisposableEvents/releases)

MIT © Lassi Harjaluoma
