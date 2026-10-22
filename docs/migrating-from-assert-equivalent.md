# Moving from `Assert.Equivalent`

xUnit.net has a built-in deep comparison, `Assert.Equivalent`, and a variant `Assert.EquivalentWithExclusions` for skipping members. Both work well for simple checks. This guide covers what `DeepAssert` adds, how to translate existing calls, and the behaviour differences to watch for.

Every example here has a runnable counterpart in [`UseCases/AssertEquivalentMigrationSamples.cs`](../tests/Xunit.Asserts.Compare.Tests/UseCases/AssertEquivalentMigrationSamples.cs).

## When it is worth switching

| | `Assert.Equivalent` | `DeepAssert` |
|---|---|---|
| Failure message | First difference only | Every difference, up to 100, each with its member path |
| Numeric and date tolerances | No: a 5 ms difference fails | `WithDoubleTolerance`, `WithDecimalTolerance`, `WithDateTimeTolerance` |
| String options | No | `IgnoreCase`, `IgnoreLeadingTrailingWhitespace`, `TreatNullAndEmptyStringAsEqual` |
| Skip members | By lambda (`EquivalentWithExclusions`) | By lambda, by name, by wildcard (`"*At"`), or by type (`IgnoreType<T>`) |
| Pair collection items by key | No | `MatchCollectionItemsBy<T>(x => x.Id)` |
| Custom equality for a type | No | `WithComparer<T>(...)` |
| Search a collection by deep equality | No: `Assert.Contains` uses `Equals` | `DeepAssert.Contains`, `DeepAssert.DoesNotContain` |

If you only need to check that an object has the right values and you rarely read failure messages, `Assert.Equivalent` is enough. Switch when failures are hard to diagnose, or when you need tolerances, string options or key-matched collections.

Some checks behave the same in both: each fails when the expected object names a member the actual object does not have (a typo in an anonymous object, for example), and each lets the actual object carry extra members.

## Translating calls

| xUnit.net | DeepAssert |
|---|---|
| `Assert.Equivalent(expected, actual)` with an anonymous or different-type `expected` | `DeepAssert.Equivalent(expected, actual)` |
| `Assert.Equivalent(expected, actual, strict: true)` with the same type on both sides | `DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder())` |
| `Assert.EquivalentWithExclusions(expected, actual, x => x.Id, x => x.CreatedAt)` | `DeepAssert.Equal(expected, actual, o => o.Ignore(x => x.Id, x => x.CreatedAt))` |
| `Assert.Contains(item, collection)` on classes without `Equals` | `DeepAssert.Contains(item, collection)` |

```csharp
// Before
Assert.EquivalentWithExclusions(expected, actual, c => c.Id, c => c.CreatedAt);

// After
DeepAssert.Equal(expected, actual, o => o.Ignore(c => c.Id, c => c.CreatedAt));
```

## Behaviour differences to watch for

### Collection order

`Assert.Equivalent` ignores the order of collection items, even in strict mode. `DeepAssert` compares collections **in order** by default. Add `IgnoreCollectionOrder()` to keep the old behaviour:

```csharp
int[] expected = [1, 2, 3];
int[] actual = [3, 1, 2];

Assert.Equivalent(expected, actual);                                    // passes
DeepAssert.Equal(expected, actual);                                     // fails: order differs
DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder());     // passes
```

### Extra items in the actual collection

In non-strict mode, `Assert.Equivalent` accepts an actual collection that holds more items than expected. `DeepAssert` requires the same items. To check "contains at least these items", assert each one:

```csharp
foreach (var item in expected)
{
    DeepAssert.Contains(item, actual);
}
```

### Extra members with different types

`Assert.Equivalent(..., strict: true)` fails when the actual object has members the expected object does not list. `DeepAssert.Equivalent` always ignores extra members on the actual object. When both sides have the same type, use `DeepAssert.Equal`, which compares every member.

### Exclusions on nested members

In `Assert.EquivalentWithExclusions`, `x => x.Address.City` excludes that one path. In `DeepAssert.Equal` it excludes `City` on every `Address` in the graph. In `DeepAssert.Equivalent`, where types differ between the two sides, it excludes every member named `City`.

### Exception type

Failures throw `EqualException` instead of `EquivalentException`. Both are standard xUnit.net exceptions, so test runners report them the same way. This only matters if a test catches the exception type explicitly.
