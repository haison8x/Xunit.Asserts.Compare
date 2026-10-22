namespace Xunit.Asserts.Compare.Tests.UseCases;

/// <summary>
/// Each pair shows an xUnit.net <c>Assert.Equivalent</c> call and its DeepAssert counterpart, as listed in
/// docs/migrating-from-assert-equivalent.md. Keep the two in sync.
/// </summary>
public class AssertEquivalentMigrationSamples
{
    private static Customer CreateCustomer() => new()
    {
        Id = Guid.Parse("8f1c2a8e-0000-4000-8000-000000000001"),
        Name = "Jacob",
        Email = "jacob@example.com",
        CreatedAt = new DateTime(2026, 1, 1, 9, 0, 0, DateTimeKind.Utc),
        ShippingAddress = new Address { Street = "1 Main St", City = "Hanoi", Country = "VN" },
        Lines =
        [
            new() { Sku = "PEN", Quantity = 10, UnitPrice = 1.20m },
            new() { Sku = "BOOK", Quantity = 1, UnitPrice = 12.50m },
        ],
    };

    [Fact]
    public void Same_type_whole_object()
    {
        var expected = CreateCustomer();
        var actual = CreateCustomer();
        actual.Lines.Reverse();

        Assert.Equivalent(expected, actual, strict: true);
        DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder());
    }

    [Fact]
    public void Anonymous_subset_of_members()
    {
        var actual = CreateCustomer();

        Assert.Equivalent(new { Name = "Jacob", ShippingAddress = new { City = "Hanoi" } }, actual);
        DeepAssert.Equivalent(new { Name = "Jacob", ShippingAddress = new { City = "Hanoi" } }, actual);
    }

    [Fact]
    public void Exclusions()
    {
        var expected = CreateCustomer();
        var actual = CreateCustomer();
        actual.Id = Guid.NewGuid();
        actual.CreatedAt = DateTime.UtcNow;

        Assert.EquivalentWithExclusions(expected, actual, c => c.Id, c => c.CreatedAt);
        DeepAssert.Equal(expected, actual, o => o.Ignore(c => c.Id, c => c.CreatedAt));
    }

    [Fact]
    public void Collection_order()
    {
        int[] expected = [1, 2, 3];
        int[] actual = [3, 1, 2];

        // Assert.Equivalent ignores order; DeepAssert compares in order unless told otherwise.
        Assert.Equivalent(expected, actual);
        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));
        DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder());
    }

    [Fact]
    public void Extra_items_in_the_actual_collection()
    {
        int[] expected = [1, 2];
        int[] actual = [1, 2, 3];

        // Non-strict Assert.Equivalent accepts extra items; DeepAssert does not, so check each expected item instead.
        Assert.Equivalent(expected, actual);
        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder()));
        foreach (var item in expected)
        {
            DeepAssert.Contains(item, actual);
        }
    }

    [Fact]
    public void Only_first_difference_versus_all_differences()
    {
        var expected = CreateCustomer();
        var actual = CreateCustomer();
        actual.Name = "James";
        actual.Email = "james@example.com";

        var xunitFailure = Assert.Throws<EquivalentException>(() => Assert.Equivalent(expected, actual));
        var deepFailure = Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));

        Assert.DoesNotContain("Email", xunitFailure.Message);
        Assert.Contains("Name", deepFailure.Message);
        Assert.Contains("Email", deepFailure.Message);
    }

    [Fact]
    public void Things_Assert_Equivalent_cannot_do()
    {
        var expected = CreateCustomer();
        var actual = CreateCustomer();
        actual.CreatedAt = expected.CreatedAt.AddMilliseconds(5);
        actual.Name = "JACOB";

        Assert.Throws<EquivalentException>(() => Assert.Equivalent(expected, actual));
        DeepAssert.Equal(expected, actual, o => o
            .WithDateTimeTolerance(TimeSpan.FromSeconds(1))
            .IgnoreCase());
    }

    [Fact]
    public void Deep_contains()
    {
        List<Customer> customers = [CreateCustomer()];

        // Assert.Contains uses Equals, which for most classes means reference equality.
        Assert.Throws<ContainsException>(() => Assert.Contains(CreateCustomer(), customers));
        DeepAssert.Contains(CreateCustomer(), customers);
    }
}
