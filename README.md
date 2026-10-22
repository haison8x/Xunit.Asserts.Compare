# xUnit.Asserts.Compare

[![NuGet](https://img.shields.io/nuget/v/xUnit.Asserts.Compare.svg)](https://www.nuget.org/packages/xUnit.Asserts.Compare)
[![NuGet downloads](https://img.shields.io/nuget/dt/xUnit.Asserts.Compare.svg)](https://www.nuget.org/packages/xUnit.Asserts.Compare)

Deep object comparison assertions for **xUnit.net v3**, powered by [CompareNETObjects](https://github.com/GregFinzer/Compare-Net-Objects).

Compare whole object graphs in one line, without overriding `Equals`. When the objects differ, the failure message lists every member that differs:

```csharp
DeepAssert.Equal(expected, actual);
```

```text
Assert.Equal() Failure: Deep comparison found 2 differences
  - Types [String,String], Item Expected.Name != Actual.Name, Values (Jacob,James)
  - Types [String,String], Item Expected.ShippingAddress.City != Actual.ShippingAddress.City, Values (Hanoi,Paris)
Expected: Customer { CreatedAt = 0001-01-01T00:00:00.0000000, Email = "jacob@example.com", Id = 00000000-0000-0000-0000-000000000000, Lines = [], Name = "Jacob", ··· }
Actual:   Customer { CreatedAt = 0001-01-01T00:00:00.0000000, Email = "jacob@example.com", Id = 00000000-0000-0000-0000-000000000000, Lines = [], Name = "James", ··· }
```

## Install

```shell
dotnet add package xUnit.Asserts.Compare
```

| Package version | xUnit.net | .NET |
|---|---|---|
| **4.x** | v3 (`xunit.v3` 4.0+) | .NET 10 |
| 2.9.x | v2 (`xunit` 2.9) | .NET 8 / .NET 9 |

Still on xUnit.net v2? Keep using `xUnit.Asserts.Compare` 2.9.2. See the [migration guide](https://github.com/haison8x/Xunit.Asserts.Compare/blob/master/docs/migration-v2-to-v4.md) when you upgrade.

## Quick start

```csharp
using Xunit.Asserts.Compare;

// Deep equality
DeepAssert.Equal(expected, actual);
DeepAssert.NotEqual(expected, actual);

// Ignore members: by expression (refactoring-safe) or by name / wildcard
DeepAssert.Equal(expected, actual, o => o.Ignore(c => c.Id, c => c.CreatedAt));
DeepAssert.Equal(expected, actual, "Id", "CreatedAt");
DeepAssert.Equal(expected, actual, o => o.Ignore("*At"));

// Collections in any order, or paired by key
DeepAssert.Equal(expectedLines, actualLines, o => o.IgnoreCollectionOrder());
DeepAssert.Equal(expectedLines, actualLines, o => o.MatchCollectionItemsBy<OrderLine>(l => l.Sku));

// Tolerances
DeepAssert.Equal(expected, actual, o => o
    .WithDoubleTolerance(1e-9)
    .WithDecimalTolerance(0.01m)
    .WithDateTimeTolerance(TimeSpan.FromSeconds(1)));

// Different types: check a DTO or API response against an anonymous object
DeepAssert.Equivalent(new { Name = "Jacob", City = "Hanoi" }, dto);

// Deep search in a collection
DeepAssert.Contains(new OrderLine { Sku = "PEN", Quantity = 10 }, lines);
DeepAssert.DoesNotContain(cancelledLine, lines);
```

## Features

| Need | API |
|---|---|
| Compare two object graphs | `DeepAssert.Equal`, `DeepAssert.NotEqual` |
| Compare objects of different types | `DeepAssert.Equivalent` |
| Find an item by deep equality | `DeepAssert.Contains`, `DeepAssert.DoesNotContain` |
| Skip members | `Ignore(x => x.Member)`, `Ignore("Name", "Type.Member", "*Suffix")`, `IgnoreType<T>()` |
| Compare only some members | `CompareOnly(x => x.Member)`, `CompareOnly("Name")` |
| Unordered collections | `IgnoreCollectionOrder()`, `MatchCollectionItemsBy<TItem>(x => x.Key)` |
| Numeric and time tolerances | `WithDoubleTolerance`, `WithDecimalTolerance`, `WithDateTimeTolerance` |
| Relaxed strings | `IgnoreCase()`, `IgnoreLeadingTrailingWhitespace()`, `TreatNullAndEmptyStringAsEqual()` |
| Custom equality for a type | `WithComparer<TValue>((a, b) => ...)` |
| Private members | `IncludePrivateMembers()` |
| Longer or shorter failure reports | `MaxDifferences(n)` (default 100) |
| Anything else CompareNETObjects supports | `Configure(config => ...)` |

## Documentation

- [Getting started](https://github.com/haison8x/Xunit.Asserts.Compare/blob/master/docs/getting-started.md): installation, first assertions, reading failure messages
- [Use cases](https://github.com/haison8x/Xunit.Asserts.Compare/blob/master/docs/use-cases.md): recipes for common testing situations
- [API reference](https://github.com/haison8x/Xunit.Asserts.Compare/blob/master/docs/api-reference.md): every assertion and option
- [Migrating from 2.x to 4.x](https://github.com/haison8x/Xunit.Asserts.Compare/blob/master/docs/migration-v2-to-v4.md)
- [Changelog](https://github.com/haison8x/Xunit.Asserts.Compare/blob/master/CHANGELOG.md)

## Building from source

Requires the .NET 10 SDK.

```shell
dotnet build
dotnet test
```

Repository layout:

```text
src/Xunit.Asserts.Compare/          the library (the NuGet package)
tests/Xunit.Asserts.Compare.Tests/  unit tests; UseCases/ holds a runnable version of every recipe in docs/use-cases.md
docs/                               documentation
Directory.Build.props               settings shared by all projects
Directory.Packages.props            NuGet package versions (central package management)
global.json                         .NET SDK version and test runner
```

## License

[MIT](https://github.com/haison8x/Xunit.Asserts.Compare/blob/master/LICENSE)
