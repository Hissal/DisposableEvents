# Working in this repository

Guidance for AI coding agents (Claude Code, Copilot, Codex, and friends) working on
DisposableEvents. Humans are welcome to read it too — it is the same information.

This is the canonical agent file. `CLAUDE.md` and `.github/copilot-instructions.md` point here;
do not duplicate content into them.

## What this repo is

A C# PubSub events library. Subscribing returns an `IDisposable`; disposing it unsubscribes. Three
NuGet packages ship from here — `DisposableEvents` (core), `DisposableEvents.R3`, and
`DisposableEvents.ZLinq` — all at one shared version.

Read [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) before making a structural change. It maps the
layers and ends with a "where do I go to change X" table.

## Commands

```bash
dotnet restore DisposableEvents.sln
```

```bash
dotnet build DisposableEvents.sln -c Release --no-restore
```

```bash
dotnet test DisposableEvents.sln -c Release --no-build --verbosity normal
```

These three are exactly what CI runs. Build the **solution**, not a single project: the libraries
multi-target down to `netstandard2.0`, and code that compiles on `net9.0` regularly does not compile
there.

The tests run on four target frameworks, each of which needs its runtime installed. If the test
host reports `error run failed: ... framework ... was not found`, that is a missing runtime on the
machine, not a failing test — fall back to a framework you do have:

```bash
dotnet test DisposableEvents.Tests/DisposableEvents.Tests.csproj -c Release -f net8.0
```

### The `-p:` trap

On Windows through Git Bash / MSYS, an MSBuild property written with a leading slash gets rewritten
as a path:

```bash
# BROKEN — MSYS turns /p:Version=1.2.3 into a bare argument named p:Version=1.2.3
dotnet build DisposableEvents.sln -c Release /p:Version=1.2.3
```

MSBuild then reports:

```
MSBUILD : error MSB1008: Only one project can be specified.
```

Use `-p:` instead. It works identically everywhere:

```bash
dotnet build DisposableEvents.sln -c Release -p:Version=1.2.3
```

(The CI workflow uses `/p:` — that is fine, it runs on Ubuntu.)

## Conventions to match

**Style.** File-scoped namespaces, 4-space indent, opening brace on the same line, `var` where the
type is obvious. Private instance fields are plain `camelCase` with no underscore (`readonly object
gate = new();`); private *static* fields use the `s_` prefix (`static IEventHub? s_hub;`). There is
no `.editorconfig` and no analyzer enforcing this — match the file you are editing.

**Nullability and language.** `LangVersion` 13, `<Nullable>enable</Nullable>`, implicit usings on.
Do not disable nullable to make a warning go away.

**Public API.** Public types carry XML doc comments where they exist today; add them for anything
new that is public. Adding a *member to a public interface* is expensive — see the multi-targeting
section of the architecture doc — so prefer an extension method in the relevant
`*SubscriberExtensions` class.

**Events and funcs are mirrored.** Nearly every file under `Events/` has a counterpart under
`Funcs/`. Change one and check the other.

**Tests.** Conventions live in [docs/TESTING.md](docs/TESTING.md) — read it, do not restate it.
Two practical notes it does not cover: the test tree mirrors the source tree
directory for directory, and while the guidelines suggest `Should_X_When_Y` naming, most existing
tests use `Member_ExpectedBehaviour` (`Publish_SendsMessageToHandlers`). Match the file you are
working in rather than reformatting its neighbours.

## Deliberate choices — do not "fix" these without asking

- **No `Directory.Build.props`, no `Directory.Packages.props`.** Package metadata is duplicated in
  each `.csproj` and `PackageReference` versions are inline, on purpose. Centralising them is a
  real proposal to discuss, not a cleanup to slip into an unrelated PR.
- **No version number in any `.csproj`.** The version is supplied at release time by the
  `Build-Release` workflow input. Never hardcode one.
- **`Events/EventTypes/LightEvent.cs` is commented-out work in progress.** Leave it.
- **`DisposableEvents.R3.csproj` links `Internal/ArrayOrOne.cs` by source.** That is how internals
  are shared without making them public. Do not "fix" it by widening the accessibility.
- **`InternalsInvisible/` and `TestPlayGround/` are empty scratch projects** in the `sandbox`
  solution folder. Not dead code to delete.
- **`.github/workflows/`** — publishing and packaging live here. Propose changes; do not make them
  as a side effect of another task.

Adding a project means registering it in `DisposableEvents.sln` under the right solution folder
(`src`, `tests`, or `sandbox`).

## Opening a pull request

Full rules in [CONTRIBUTING.md](CONTRIBUTING.md). The parts agents get wrong most often:

1. **Never push to `main`.** Release notes are generated from merged PRs; a direct push is invisible
   in the changelog permanently.
2. **The PR title is a published changelog line.** Conventional Commits, imperative, lowercase after
   the colon, no trailing period: `fix: dispose pooled handlers when clearing`. Use `feat`, not
   `feature`.
3. **Apply a label** — `breaking-change`, `enhancement`, `bug`, `documentation`, or `dependencies`.
   Unlabelled PRs are dumped into "Other Changes".
4. Run the build and the tests before claiming the work is done, and report the actual result.
