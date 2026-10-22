# API reference

Namespace: `Xunit.Asserts.Compare`

- [`DeepAssert`](#deepassert)
- [`DeepAssertOptions<T>`](#deepassertoptionst)
- [How comparison works](#how-comparison-works)

---

## `DeepAssert`

Static class with the assertions. All of them throw xUnit.net's standard exception types from `Xunit.Sdk`.

### `Equal`

```csharp
void Equal<T>(T expected, T actual, params string[] propertiesToIgnore)
void Equal<T>(T expected, T actual, Action<DeepAssertOptions<T>> configure)
```

Passes when `expected` and `actual` are deeply equal. Throws `EqualException` otherwise. The message lists up to `MaxDifferences` differences (100 by default).

The first overload is kept from 2.x. `propertiesToIgnore` accepts the same names as [`Ignore(params string[])`](#ignore). It is equivalent to `o => o.Ignore(propertiesToIgnore)`.

Two `null` values are equal. A `null` and a non-null value are not.

### `NotEqual`

```csharp
void NotEqual<T>(T expected, T actual, params string[] propertiesToIgnore)
void NotEqual<T>(T expected, T actual, Action<DeepAssertOptions<T>> configure)
```

Passes when the objects differ in at least one compared member. Throws `NotEqualException` when they are deeply equal. Ignored members do not count as differences.

### `Equivalent`

```csharp
void Equivalent<TExpected, TActual>(TExpected expected, TActual actual, Action<DeepAssertOptions<TActual>>? configure = null)
```

Compares objects whose types may differ, matching members by name. This is typically used for an anonymous object against a DTO or entity.

- The members of `expected` drive the comparison. Members that exist only on `actual` are not checked.
- Every member of `expected` must exist on `actual`, at every level of nesting. Otherwise `EqualException` lists the missing paths (for example `Address.Zip` or `Orders[0].Sku`). Call `IgnoreMissingMembers()` to skip such members instead.
- Member expressions in `configure` are written against `TActual`.
- Options otherwise behave as for `Equal`. `IgnoreObjectTypes()` is turned on automatically.

### `Contains` / `DoesNotContain`

```csharp
void Contains<T>(T expected, IEnumerable<T> collection, Action<DeepAssertOptions<T>>? configure = null)
void DoesNotContain<T>(T expected, IEnumerable<T> collection, Action<DeepAssertOptions<T>>? configure = null)
```

`Contains` passes when at least one item is deeply equal to `expected`, and throws `ContainsException` otherwise. `DoesNotContain` passes when no item is deeply equal, and otherwise throws `DoesNotContainException`, which points at the index of the first match. The collection is enumerated once. A `null` collection throws `ArgumentNullException`.

---

## `DeepAssertOptions<T>`

Passed to the `configure` callback. Every method returns the same instance, so calls can be chained. Invalid arguments throw `ArgumentException` (or `ArgumentNullException` / `ArgumentOutOfRangeException`) as soon as the method is called.

### Selecting members

<a id="ignore"></a>

| Method | Effect |
|---|---|
| `Ignore(params Expression<Func<T, object?>>[] members)` | Skips the given members. A nested path such as `x => x.Address.City` skips `City` on every `Address`, while `City` on other types is still compared. Inherited members work. |
| `Ignore(params string[] memberNames)` | Skips members by name: plain (`"Id"`, every type), type-qualified (`"Order.Id"`) or wildcard (`"*At"`, `"Audit*"`, `"*Temp*"`). Case sensitive. |
| `IgnoreType<TIgnore>()` | Skips every value of class type `TIgnore` anywhere in the graph. |
| `CompareOnly(params Expression<Func<T, object?>>[] members)` | Compares only these members. Every member on a nested path is included, so `x => x.Address.City` includes `Address` and `City`. |
| `CompareOnly(params string[] memberNames)` | Compares only the members with these names. |

`CompareOnly` works by member name at every level. Including `Address` alone does not include the members of `Address`, so name each nested member you want compared.

### Collections

| Method | Effect |
|---|---|
| `IgnoreCollectionOrder()` | Collections are equal when they hold the same items in any order. |
| `MatchCollectionItemsBy<TItem>(params Expression<Func<TItem, object?>>[] keys)` | Ignores order and pairs `TItem` items by the given direct members (for example `x => x.Id`). Paired items are then compared member by member, so failures name the differing member. |

### Tolerances

| Method | Effect |
|---|---|
| `WithDoubleTolerance(double tolerance)` | `double` and `float` values within `tolerance` of each other are equal. Must be finite and non-negative. |
| `WithDecimalTolerance(decimal tolerance)` | The same for `decimal`. Must be non-negative. |
| `WithDateTimeTolerance(TimeSpan tolerance)` | The same for `DateTime` and `DateTimeOffset`, with millisecond resolution. Must be between zero and `int.MaxValue` milliseconds. |

### Strings

| Method | Effect |
|---|---|
| `IgnoreCase()` | Case-insensitive comparison. |
| `IgnoreLeadingTrailingWhitespace()` | Trims both strings before comparing. |
| `TreatNullAndEmptyStringAsEqual()` | `null` equals `""`. |

### Types and visibility

| Method | Effect |
|---|---|
| `IgnoreObjectTypes()` | Compares members even when the runtime types differ. `Equivalent` turns this on for you. |
| `IgnoreMissingMembers()` | Used with `Equivalent`: members of `expected` that `actual` lacks are skipped instead of failing. |
| `IncludePrivateMembers()` | Also compares private properties and fields. |

### Custom logic

| Method | Effect |
|---|---|
| `WithComparer<TValue>(Func<TValue, TValue, bool> comparer)` | Uses `comparer` for every `TValue` (exact type match). Two `null`s are equal and `null` vs non-null is not, so the delegate only sees non-null values. |
| `MaxDifferences(int maxDifferences)` | Stops after this many differences. The default is `DeepAssertOptions<T>.DefaultMaxDifferences` (100). Must be at least 1. |
| `Configure(Action<ComparisonConfig> configure)` | Edits the CompareNETObjects [`ComparisonConfig`](https://github.com/GregFinzer/Compare-Net-Objects/wiki/Configuration) directly. Callbacks run after all other options, in the order added. |

---

## How comparison works

- Comparison is done by [CompareNETObjects](https://github.com/GregFinzer/Compare-Net-Objects). It walks public properties and fields recursively, and handles collections, dictionaries and circular references.
- Equality is decided member by member, so the compared types do not need to override `Equals`. Use `WithComparer<T>` when a type needs its own definition of equality.
- Collections are compared in order unless `IgnoreCollectionOrder` or `MatchCollectionItemsBy` is set.
- Each assertion builds a fresh configuration. Options never leak between assertions or between tests running in parallel.
- `Contains` and `DoesNotContain` stop comparing an item at its first difference, because they only need a yes or no answer per item.
