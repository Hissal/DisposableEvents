<!--
  The PR title is published verbatim as a line in the next release's changelog.
  Format: <type>: <imperative summary, lowercase, no trailing period>
  Types: feat | fix | docs | refactor | perf | test | build | ci | chore   (feat, not feature)
  Breaking change? Add a "!" -> feat!: ...
-->

## What

<!-- What changes, and why. Link any related issue. -->

## Checklist

- [ ] **Label applied** — `breaking-change`, `enhancement`, `bug`, `documentation`, or
      `dependencies`. Without one this PR lands in "Other Changes" in the release notes.
- [ ] PR title follows `<type>: <summary>` and reads well as a changelog line
- [ ] `dotnet build DisposableEvents.sln -c Release` passes
- [ ] `dotnet test DisposableEvents.sln -c Release --no-build` passes
- [ ] Tests added or updated for behaviour changes (see
      [TestGuidelines.md](../DisposableEvents.Tests/TestGuidelines.md))
- [ ] Public API changes carry XML doc comments, and the `Events` / `Funcs` mirror was checked

See [CONTRIBUTING.md](../CONTRIBUTING.md) for the full rules.
