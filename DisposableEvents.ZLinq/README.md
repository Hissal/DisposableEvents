# DisposableEvents.ZLinq

[ZLinq](https://github.com/Cysharp/ZLinq) integration for
[DisposableEvents](https://github.com/Hissal/DisposableEvents). Query the results of a func
publisher through ZLinq's zero-allocation `ValueEnumerable<T>` instead of LINQ over `IEnumerable`.

## Install

```bash
dotnet add package DisposableEvents.ZLinq
```

Brings `DisposableEvents` and `ZLinq` with it. Targets `net9.0`, `net8.0`, `netstandard2.1`, and
`netstandard2.0`.

## Usage

A func has many handlers, and each returns a `FuncResult<T>` that may or may not hold a value. This
package streams those results:

```csharp
using DisposableEvents;
using DisposableEvents.ZLinq;
using ZLinq;

var priceQuote = new DisposableFunc<string, decimal>();

priceQuote.AddHandler(item => item == "sword" ? 120m : FuncResult<decimal>.Null());
priceQuote.AddHandler(item => 99m);

var quotes = priceQuote.InvokeAsValueEnumerable("sword")
    .GetValuesOrDefault(0m)     // unwrap, skipping results with no value
    .ToArray();
```

Two entry points, differing in *when* the handlers run:

| Method | Behaviour |
|---|---|
| `InvokeAsValueEnumerable(arg)` | Deferred — each handler runs as you iterate, so stopping early skips the rest |
| `InvokeAsValueEnumerableImmediate(arg)` | Eager — every handler runs up front, results buffered |

Reach for the deferred form when you plan to short-circuit (`First`, `Take`, `Any`), and the
immediate form when you need every handler invoked regardless of how the sequence is consumed.

The `FuncResult` operators from the core library are mirrored here in ZLinq terms — `Combine`,
`CombineValues`, `FirstValueOrDefault`, `ForEach`, `ForEachValue`, `GetValues`,
`GetValuesOrDefault`, `Match`, and `Switch`.

## Documentation

[Main repository and README](https://github.com/Hissal/DisposableEvents) ·
[Architecture](https://github.com/Hissal/DisposableEvents/blob/main/docs/ARCHITECTURE.md) ·
[Releases](https://github.com/Hissal/DisposableEvents/releases)

MIT © Lassi Harjaluoma
