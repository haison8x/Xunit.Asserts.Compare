namespace Xunit.Asserts.Compare.Tests;

public class NotEqualTests
{
    [Fact]
    public void Passes_when_objects_differ()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Address!.Street = "2 Main St";

        DeepAssert.NotEqual(expected, actual);
    }

    [Fact]
    public void Passes_when_one_side_is_null()
    {
        DeepAssert.NotEqual<Person?>(null, TestData.CreatePerson());
    }

    [Fact]
    public void Fails_when_objects_are_deeply_equal()
    {
        Assert.Throws<NotEqualException>(() => DeepAssert.NotEqual(TestData.CreatePerson(), TestData.CreatePerson()));
    }

    [Fact]
    public void Fails_when_both_are_null()
    {
        Assert.Throws<NotEqualException>(() => DeepAssert.NotEqual<Person?>(null, null));
    }

    [Fact]
    public void Failure_message_shows_the_real_values()
    {
        // Regression: 2.x passed the literal strings "expected" and "actual" instead of the values.
        var ex = Assert.Throws<NotEqualException>(
            () => DeepAssert.NotEqual(new PersonDto { Id = 7, Name = "Jacob" }, new PersonDto { Id = 7, Name = "Jacob" }));

        Assert.Contains("Assert.NotEqual() Failure", ex.Message);
        Assert.Contains("Jacob", ex.Message);
        Assert.DoesNotContain("Expected: expected", ex.Message);
    }

    [Fact]
    public void Legacy_overload_fails_when_only_ignored_members_differ()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";

        Assert.Throws<NotEqualException>(() => DeepAssert.NotEqual(expected, actual, "Name"));
    }

    [Fact]
    public void Options_overload_fails_when_only_ignored_members_differ()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";

        Assert.Throws<NotEqualException>(() => DeepAssert.NotEqual(expected, actual, o => o.Ignore(p => p.Name)));
    }

    [Fact]
    public void Options_overload_passes_when_non_ignored_members_differ()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";
        actual.Age = 31;

        DeepAssert.NotEqual(expected, actual, o => o.Ignore(p => p.Name));
    }
}
