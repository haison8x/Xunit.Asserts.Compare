# Changelog

## 4.0.0

Requires xUnit.net v3 and .NET 10. See [docs/migration-v2-to-v4.md](docs/migration-v2-to-v4.md).

### Added

- `Equal` and `NotEqual` overloads that take `Action<DeepAssertOptions<T>>`.
- `DeepAssertOptions<T>` with these options:
  - Ignoring and selecting members: `Ignore` (by expression or by name/wildcard), `IgnoreType<T>`, `CompareOnly`.
  - Collections: `IgnoreCollectionOrder`, `MatchCollectionItemsBy<TItem>`.
  - Tolerances: `WithDoubleTolerance`, `WithDecimalTolerance`, `WithDateTimeTolerance`.
  - Strings: `IgnoreCase`, `IgnoreLeadingTrailingWhitespace`, `TreatNullAndEmptyStringAsEqual`.
  - Types and visibility: `IgnoreObjectTypes`, `IgnoreMissingMembers`, `IncludePrivateMembers`.
  - Custom logic: `WithComparer<TValue>`, `MaxDifferences`, and `Configure` for direct access to CompareNETObjects.
- `DeepAssert.Equivalent` for comparing objects of different types, such as an anonymous object against a DTO. It fails when the expected object has a member that the actual object lacks.
- `DeepAssert.Contains` and `DeepAssert.DoesNotContain` for finding items by deep equality.
- XML documentation for IntelliSense, and README and docs included in the package.
- Guide for moving from xUnit.net's built-in `Assert.Equivalent`.

### Changed

- Depends on `xunit.v3.assert` 4.0.0 or later (works with any `xunit.v3` 4.x) instead of the `xunit` 2.9.2 meta-package.
- Targets `net10.0` only.
- Updated `CompareNETObjects` to 4.84.0.
- Failure messages list up to 100 differences (previously 1), one per line.
- Assembly renamed from `xUnit.Asserts.Compare.dll` to `Xunit.Asserts.Compare.dll` to match the `Xunit.Asserts.Compare` namespace. The NuGet package ID is unchanged.
- Licensed under MIT; the package now declares its license.
- Repository reorganised into `src/`, `tests/` and `docs/`, with shared build settings and central package management.

### Fixed

- The `NotEqual` failure message showed the literal text `expected` / `actual` instead of the values.

## 2.9.2

- Target `net8.0` and `net9.0`, xUnit.net 2.9.2, CompareNETObjects 4.83.0.
