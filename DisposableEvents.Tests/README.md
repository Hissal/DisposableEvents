# DisposableEvents.Tests

Test conventions live in **[docs/TESTING.md](../docs/TESTING.md)** — naming, structure, coverage
expectations, and which tools to reach for.

This tree mirrors the source tree directory for directory: a test for
`DisposableEvents/Events/EventTypes/BufferedEvent.cs` belongs in
`Events/EventTypes/BufferedEventTest.cs`.

Run everything the way CI does:

```bash
dotnet test DisposableEvents.sln -c Release
```

Or one target framework at a time — the project builds against `net6.0`, `net8.0`, `net9.0`, and
`net48` on Windows, and each needs its runtime installed:

```bash
dotnet test DisposableEvents.Tests/DisposableEvents.Tests.csproj -c Release -f net8.0
```

See [CONTRIBUTING.md](../CONTRIBUTING.md) for the full build and PR workflow.
