using KellermanSoftware.CompareNetObjects;
using Xunit.Asserts.Compare.Internal;

namespace Xunit.Asserts.Compare.Tests;

public class MissingMemberFinderTests
{
    private static IReadOnlyList<string> Find(object? expected, object? actual, Action<ComparisonConfig>? configure = null)
    {
        var config = new ComparisonConfig { IgnoreObjectTypes = true };
        configure?.Invoke(config);
        return MissingMemberFinder.Find(expected, actual, config);
    }

    [Fact]
    public void Returns_nothing_when_either_side_is_null()
    {
        Assert.Empty(Find(null, new PersonDto()));
        Assert.Empty(Find(new { Extra = 1 }, null));
    }

    [Fact]
    public void Returns_nothing_for_simple_values()
    {
        Assert.Empty(Find("text", 5));
        Assert.Empty(Find(Guid.Empty, DateTime.MinValue));
    }

    [Fact]
    public void Walks_dictionary_values_for_shared_keys()
    {
        var expected = new Dictionary<string, object> { ["a"] = new { Id = 1, Extra = 2 }, ["only-expected"] = new { Nope = 1 } };
        var actual = new Dictionary<string, object> { ["a"] = new PersonDto { Id = 1 } };

        Assert.Equal(["[a].Extra"], Find(expected, actual));
    }

    [Fact]
    public void Stops_walking_a_collection_when_actual_runs_out_of_items()
    {
        var expected = new object[] { new { Id = 1 }, new { Missing = 1 } };
        var actual = new[] { new PersonDto { Id = 1 } };

        Assert.Empty(Find(expected, actual));
    }

    [Fact]
    public void Skips_members_outside_the_include_list()
    {
        Assert.Empty(Find(new { Id = 1, Extra = 2 }, new PersonDto(), c => c.MembersToInclude.Add("Id")));
    }

    [Fact]
    public void Skips_members_whose_type_is_ignored()
    {
        Assert.Empty(Find(new { Id = 1, Audit = new AuditInfo() }, new PersonDto(), c => c.ClassTypesToIgnore.Add(typeof(AuditInfo))));
    }

    [Theory]
    [InlineData("Extra")]
    [InlineData("*tra")]
    [InlineData("Ext*")]
    [InlineData("*xtr*")]
    public void Skips_members_matching_an_ignore_pattern(string pattern)
    {
        Assert.Empty(Find(new { Id = 1, Extra = 2 }, new PersonDto(), c => c.MembersToIgnore.Add(pattern)));
    }

    [Theory]
    [InlineData("*")]
    [InlineData("Other")]
    [InlineData("*zzz")]
    public void Reports_members_not_matching_an_ignore_pattern(string pattern)
    {
        Assert.Equal(["Extra"], Find(new { Id = 1, Extra = 2 }, new PersonDto(), c => c.MembersToIgnore.Add(pattern)));
    }

    [Fact]
    public void Skips_members_ignored_by_type_qualified_name()
    {
        var expected = new Employee { Department = "IT" };

        Assert.Empty(Find(expected, new PersonDto(), c => c.MembersToIgnore.AddRange(["Employee.Department", "Person.CreatedAt", "Address", "Orders", "Scores"])));
    }

    [Fact]
    public void Checks_private_members_only_when_enabled()
    {
        var expected = new SecretHolder("x");
        var actual = new { Public = "same" };

        Assert.Empty(Find(expected, actual));
        Assert.Contains("_secret", Find(expected, actual, c => c.ComparePrivateFields = true));
    }

    [Fact]
    public void Skips_properties_or_fields_when_their_comparison_is_disabled()
    {
        Assert.Empty(Find(new { Extra = 1 }, new PersonDto(), c => c.CompareProperties = false));
        Assert.Empty(Find(new FieldHolder(), new PersonDto(), c => c.CompareFields = false));
        Assert.Equal(["Value"], Find(new FieldHolder(), new PersonDto()));
    }

    [Fact]
    public void Ignores_members_whose_getter_throws()
    {
        Assert.Empty(Find(new { Thrower = new ThrowingHolder() }, new { Thrower = new ThrowingHolder() }));
    }

    private sealed class FieldHolder
    {
        public int Value = 1;
    }

    private sealed class ThrowingHolder
    {
        public Address Broken => throw new InvalidOperationException("boom");
    }
}
