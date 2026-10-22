# Migrating from 2.x to 4.x

Version 4 moves the library to xUnit.net v3 and .NET 10. Your existing `DeepAssert.Equal` and `DeepAssert.NotEqual` calls keep compiling unchanged.

## Should you upgrade?

| Your test project uses | Use |
|---|---|
| `xunit` 2.x (xUnit.net v2) | `xUnit.Asserts.Compare` **2.9.2** |
| `xunit.v3` 4.x on .NET 10 | `xUnit.Asserts.Compare` **4.x** |

## Steps

1. Move the test project to xUnit.net v3. Follow the [official guide](https://xunit.net/docs/getting-started/v3/migration): replace `xunit` with `xunit.v3`, set `<OutputType>Exe</OutputType>` and update `xunit.runner.visualstudio`.
2. Target `net10.0`.
3. On the .NET 10 SDK, add a `global.json` that selects Microsoft Testing Platform (see [Getting started](getting-started.md#install)).
4. Update the package: `dotnet add package xUnit.Asserts.Compare --version 4.0.0`.

## Behaviour changes

| Area | 2.x | 4.x |
|---|---|---|
| Dependencies | `xunit` meta-package 2.9.2 (core, runner and assertions) | `xunit.v3.assert` 4.0.0 or later only, so any `xunit.v3` 4.x works |
| Target framework | `net8.0`, `net9.0` | `net10.0` |
| Assembly file | `xUnit.Asserts.Compare.dll` | `Xunit.Asserts.Compare.dll`. The package ID and namespace are unchanged |
| Differences in a failure message | Only the first difference, in CompareNETObjects' raw format | Up to 100 differences, one per line. Change the limit with `MaxDifferences(n)` |
| `NotEqual` failure message | Showed the literal text `expected` / `actual` | Shows the formatted objects |
| Ignore names in the legacy overload | Anything accepted | `null`, empty or whitespace-only names throw `ArgumentException` |

The exception types (`EqualException` and `NotEqualException`) are unchanged.

## Worth adopting

Expressions are checked by the compiler, unlike string names:

```csharp
// 2.x
DeepAssert.Equal(expected, actual, "Id", "CreatedAt");

// 4.x
DeepAssert.Equal(expected, actual, o => o.Ignore(c => c.Id, c => c.CreatedAt));
```

See [Use cases](use-cases.md) for the new options: unordered collections, tolerances, `Equivalent`, `Contains` and others.
