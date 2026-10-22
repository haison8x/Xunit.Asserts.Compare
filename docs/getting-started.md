# Getting started

## Requirements

- .NET 10
- xUnit.net v3 (`xunit.v3` package, version 4.0 or later)

The library depends only on `xunit.v3.assert` and `CompareNETObjects`. It does not bring in the xUnit.net runner or core, so it does not conflict with what your test project already references.

## Install

```shell
dotnet add package xUnit.Asserts.Compare
```

A typical xUnit.net v3 test project then looks like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <OutputType>Exe</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="xunit.v3" Version="4.0.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="4.0.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.11.0" />
    <PackageReference Include="xUnit.Asserts.Compare" Version="4.0.0" />
  </ItemGroup>
</Project>
```

On the .NET 10 SDK, `dotnet test` needs Microsoft Testing Platform mode for xUnit.net v3. Add a `global.json` next to your solution:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

## Your first deep assertion

```csharp
using Xunit;
using Xunit.Asserts.Compare;

public class StudentTests
{
    [Fact]
    public void Students_with_the_same_data_are_equal()
    {
        var expected = new Student { Id = 1, Name = "Jacob" };
        var actual = new Student { Id = 1, Name = "Jacob" };

        DeepAssert.Equal(expected, actual);
    }
}
```

`Assert.Equal` would fail here, because `Student` does not override `Equals` and the two variables are different instances. `DeepAssert.Equal` instead compares the objects member by member: public properties and fields, nested objects, collections and dictionaries, recursively.

## Adding options

Every assertion has an overload that takes a configuration callback. The callback receives a `DeepAssertOptions<T>`, whose methods can be chained:

```csharp
DeepAssert.Equal(expected, actual, o => o
    .Ignore(s => s.Id)
    .IgnoreCollectionOrder()
    .WithDateTimeTolerance(TimeSpan.FromSeconds(1)));
```

Member expressions such as `s => s.Id` are checked by the compiler, so they keep working when you rename a property. The [API reference](api-reference.md) lists every option.

## Reading a failure message

```text
Assert.Equal() Failure: Deep comparison found 2 differences
  - Types [String,String], Item Expected.Name != Actual.Name, Values (Jacob,James)
  - Types [String,String], Item Expected.ShippingAddress.City != Actual.ShippingAddress.City, Values (Hanoi,Paris)
Expected: Customer { CreatedAt = 0001-01-01T00:00:00.0000000, Email = "jacob@example.com", Id = 00000000-0000-0000-0000-000000000000, Lines = [], Name = "Jacob", ··· }
Actual:   Customer { CreatedAt = 0001-01-01T00:00:00.0000000, Email = "jacob@example.com", Id = 00000000-0000-0000-0000-000000000000, Lines = [], Name = "James", ··· }
```

- The first line gives the number of differences found.
- Each `-` line is one difference. It shows the member path (`Expected.ShippingAddress.City`), the types on both sides and the two values. Collection items show their index (`Expected.Lines[2]`), or their key when matched with `MatchCollectionItemsBy` (`Expected[Sku:PEN].Quantity`).
- `Expected:` and `Actual:` are xUnit.net's usual summary of both objects.

Up to 100 differences are reported by default. When there are more, the header says `(stopped after 100 differences; call MaxDifferences(n) to report more)`.

The exception types are xUnit.net's own (`EqualException`, `NotEqualException`, `ContainsException`, `DoesNotContainException`), so test runners and IDEs display them like any other assertion failure.

## Next steps

- [Use cases](use-cases.md): recipes for ignoring generated ids, checking API responses, unordered query results and more
- [API reference](api-reference.md)
