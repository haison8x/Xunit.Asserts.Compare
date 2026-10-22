namespace Xunit.Asserts.Compare.Tests;

public class EqualTests
{
    [Fact]
    public void Passes_for_identical_object_graphs()
    {
        DeepAssert.Equal(TestData.CreatePerson(), TestData.CreatePerson());
    }

    [Fact]
    public void Passes_for_same_reference()
    {
        var person = TestData.CreatePerson();

        DeepAssert.Equal(person, person);
    }

    [Fact]
    public void Passes_when_both_are_null()
    {
        DeepAssert.Equal<Person?>(null, null);
    }

    [Fact]
    public void Fails_when_expected_is_null_and_actual_is_not()
    {
        Assert.Throws<EqualException>(() => DeepAssert.Equal<Person?>(null, TestData.CreatePerson()));
    }

    [Fact]
    public void Fails_when_actual_is_null_and_expected_is_not()
    {
        Assert.Throws<EqualException>(() => DeepAssert.Equal<Person?>(TestData.CreatePerson(), null));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    [InlineData(-5, -5)]
    public void Passes_for_equal_primitives(int expected, int actual)
    {
        DeepAssert.Equal(expected, actual);
    }

    [Fact]
    public void Fails_for_different_primitives()
    {
        Assert.Throws<EqualException>(() => DeepAssert.Equal(1, 2));
    }

    [Fact]
    public void Fails_for_different_top_level_property()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));
    }

    [Fact]
    public void Fails_for_different_nested_property()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Address!.City = "Paris";

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));
    }

    [Fact]
    public void Fails_for_different_collection_item()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Orders[1].Product = "Pencil";

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));
    }

    [Fact]
    public void Fails_for_different_collection_length()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Orders.Add(new Order { Id = 12 });

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));
    }

    [Fact]
    public void Fails_for_different_dictionary_value()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Scores["math"] = 10;

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));
    }

    [Fact]
    public void Compares_collections_in_order_by_default()
    {
        int[] expected = [1, 2, 3];
        int[] actual = [3, 2, 1];

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));
    }

    [Fact]
    public void Passes_for_equal_anonymous_objects()
    {
        DeepAssert.Equal(new { Id = 1, Tags = new[] { "a", "b" } }, new { Id = 1, Tags = new[] { "a", "b" } });
    }

    [Fact]
    public void Failure_message_names_the_differing_member_and_values()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Address!.City = "Paris";

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));

        Assert.Contains("Assert.Equal() Failure", ex.Message);
        Assert.Contains("1 difference", ex.Message);
        Assert.Contains("Address.City", ex.Message);
        Assert.Contains("Hanoi", ex.Message);
        Assert.Contains("Paris", ex.Message);
        Assert.Contains("Expected:", ex.Message);
        Assert.Contains("Actual:", ex.Message);
    }

    [Fact]
    public void Failure_message_lists_every_difference()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";
        actual.Age = 31;
        actual.Address!.City = "Paris";

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual));

        Assert.Contains("3 differences", ex.Message);
        Assert.Contains(".Name", ex.Message);
        Assert.Contains(".Age", ex.Message);
        Assert.Contains(".Address.City", ex.Message);
    }

    [Fact]
    public void Legacy_overload_ignores_members_by_name()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";
        actual.Age = 99;

        DeepAssert.Equal(expected, actual, "Name", "Age");
    }

    [Fact]
    public void Legacy_overload_still_fails_on_members_not_ignored()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";
        actual.Age = 99;

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, "Name"));
    }

    [Fact]
    public void Options_overload_with_no_options_behaves_like_default()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";

        DeepAssert.Equal(TestData.CreatePerson(), TestData.CreatePerson(), _ => { });
        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, _ => { }));
    }

    [Fact]
    public void Options_overload_rejects_null_configure_delegate()
    {
        Assert.Throws<ArgumentNullException>(
            () => DeepAssert.Equal(1, 1, (Action<DeepAssertOptions<int>>)null!));
    }
}
