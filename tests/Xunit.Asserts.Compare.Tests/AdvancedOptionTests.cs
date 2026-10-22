using KellermanSoftware.CompareNetObjects;

namespace Xunit.Asserts.Compare.Tests;

public class AdvancedOptionTests
{
    [Fact]
    public void WithComparer_overrides_comparison_for_a_type()
    {
        var expected = new PersonDto { Id = 1, Name = "Jacob" };
        var actual = new PersonDto { Id = 1, Name = "Jacob (draft)" };

        DeepAssert.Equal(expected, actual, o => o.WithComparer<string>((a, b) => a.Split(' ')[0] == b.Split(' ')[0]));
    }

    [Fact]
    public void WithComparer_reports_a_difference_when_the_delegate_returns_false()
    {
        var expected = new PersonDto { Id = 1, Name = "Jacob" };
        var actual = new PersonDto { Id = 1, Name = "James" };

        Assert.Throws<EqualException>(
            () => DeepAssert.Equal(expected, actual, o => o.WithComparer<string>((a, b) => a.Length == b.Length && a[0] == 'X')));
    }

    [Fact]
    public void WithComparer_applies_to_nested_objects()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Address = new Address { Street = "other", City = "HANOI" };

        DeepAssert.Equal(
            expected,
            actual,
            o => o.WithComparer<Address>((a, b) => string.Equals(a.City, b.City, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Default_reports_up_to_100_differences()
    {
        var expected = Enumerable.Range(0, 150).ToArray();
        var actual = Enumerable.Range(1, 150).ToArray();

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));

        Assert.Contains("100 differences", ex.Message);
        Assert.Contains("MaxDifferences", ex.Message);
    }

    [Fact]
    public void MaxDifferences_limits_the_differences_reported()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";
        actual.Age = 31;
        actual.Address!.City = "Paris";

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.MaxDifferences(1)));

        Assert.Contains("1 difference", ex.Message);
        Assert.Contains("stopped after 1", ex.Message);
    }

    [Fact]
    public void MaxDifferences_must_be_positive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DeepAssertOptions<Person>().MaxDifferences(0));
    }

    [Fact]
    public void IncludePrivateMembers_compares_private_fields()
    {
        var expected = new SecretHolder("a");
        var actual = new SecretHolder("b");

        DeepAssert.Equal(expected, actual);
        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.IncludePrivateMembers()));
    }

    [Fact]
    public void Configure_gives_access_to_the_underlying_ComparisonConfig()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Orders.Clear();

        DeepAssert.Equal(expected, actual, o => o.Configure(c => c.TypesToIgnore.Add(typeof(List<Order>))));
    }

    [Fact]
    public void Configure_runs_after_fluent_options()
    {
        var expected = new PersonDto { Name = "Jacob" };
        var actual = new PersonDto { Name = "JACOB" };

        Assert.Throws<EqualException>(
            () => DeepAssert.Equal(expected, actual, o => o.Configure(c => c.CaseSensitive = true).IgnoreCase()));
    }

    [Fact]
    public void Options_can_be_reused_across_assertions()
    {
        Action<DeepAssertOptions<Person>> ignoreAudit = o => o.Ignore(p => p.CreatedAt, p => p.Id);

        var first = TestData.CreatePerson();
        first.Id = 5;
        var second = TestData.CreatePerson();
        second.CreatedAt = DateTime.MinValue;

        DeepAssert.Equal(TestData.CreatePerson(), first, ignoreAudit);
        DeepAssert.Equal(TestData.CreatePerson(), second, ignoreAudit);
    }

    [Fact]
    public void Configure_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => new DeepAssertOptions<Person>().Configure((Action<ComparisonConfig>)null!));
    }
}
