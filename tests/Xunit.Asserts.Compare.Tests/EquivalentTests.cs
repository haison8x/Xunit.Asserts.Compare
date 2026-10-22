namespace Xunit.Asserts.Compare.Tests;

public class EquivalentTests
{
    [Fact]
    public void Passes_for_different_types_with_equal_members()
    {
        var entity = new PersonDto { Id = 1, Name = "Jacob", Age = 30 };
        var expected = new { Id = 1, Name = "Jacob", Age = 30 };

        DeepAssert.Equivalent(expected, entity);
    }

    [Fact]
    public void Fails_when_a_shared_member_differs()
    {
        var entity = new PersonDto { Id = 1, Name = "Jacob", Age = 30 };
        var expected = new { Id = 1, Name = "James", Age = 30 };

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equivalent(expected, entity));

        Assert.Contains("Name", ex.Message);
    }

    [Fact]
    public void Members_only_on_actual_are_not_checked()
    {
        var entity = new PersonDto { Id = 1, Name = "Jacob", Age = 30 };

        DeepAssert.Equivalent(new { Id = 1, Name = "Jacob" }, entity);
    }

    [Fact]
    public void Fails_when_an_expected_member_does_not_exist_on_actual()
    {
        var entity = new PersonDto { Id = 1, Name = "Jacob", Age = 30 };

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equivalent(new { Id = 1, Nmae = "Jacob" }, entity));

        Assert.Contains("1 expected member does not exist on PersonDto", ex.Message);
        Assert.Contains("- Nmae", ex.Message);
    }

    [Fact]
    public void Reports_missing_members_with_their_nested_path()
    {
        var person = TestData.CreatePerson();
        var expected = new { Id = 1, Address = new { City = "Hanoi", Zip = "10000" }, Orders = new[] { new { Id = 10, Sku = "B-1" } } };

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equivalent(expected, person));

        Assert.Contains("2 expected members do not exist", ex.Message);
        Assert.Contains("- Address.Zip", ex.Message);
        Assert.Contains("- Orders[0].Sku", ex.Message);
    }

    [Fact]
    public void Missing_member_check_honours_ignored_members()
    {
        var entity = new PersonDto { Id = 1, Name = "Jacob", Age = 30 };

        DeepAssert.Equivalent(new { Id = 1, Name = "Jacob", Extra = 5 }, entity, o => o.Ignore("Extra"));
    }

    [Fact]
    public void IgnoreMissingMembers_skips_expected_members_that_actual_lacks()
    {
        var entity = new PersonDto { Id = 1, Name = "Jacob", Age = 30 };

        DeepAssert.Equivalent(new { Id = 1, Name = "Jacob", Extra = 5 }, entity, o => o.IgnoreMissingMembers());
    }

    [Fact]
    public void Compares_collections_of_different_item_types()
    {
        var person = TestData.CreatePerson();
        var expected = new[] { new { Id = 10, Product = "Book" }, new { Id = 11, Product = "Pen" } };

        DeepAssert.Equivalent(expected, person.Orders);

        expected[1] = new { Id = 11, Product = "Pencil" };
        Assert.Throws<EqualException>(() => DeepAssert.Equivalent(expected, person.Orders));
    }

    [Fact]
    public void Handles_object_graphs_with_cycles()
    {
        var first = new Node { Name = "a" };
        first.Next = first;
        var second = new Node { Name = "a" };
        second.Next = second;

        DeepAssert.Equivalent(first, second);
    }

    private sealed class Node
    {
        public string? Name { get; set; }

        public Node? Next { get; set; }
    }

    [Fact]
    public void Compares_nested_objects_of_different_types()
    {
        var person = TestData.CreatePerson();
        var expected = new { Street = "1 Main St", City = "Hanoi" };

        DeepAssert.Equivalent(expected, person.Address);
    }

    [Fact]
    public void Ignore_by_name_works_across_types()
    {
        var entity = new PersonDto { Id = 1, Name = "Jacob", Age = 30 };

        DeepAssert.Equivalent(new { Id = 99, Name = "Jacob", Age = 30 }, entity, o => o.Ignore("Id"));
    }

    [Fact]
    public void Ignore_by_expression_on_the_actual_type_works_across_types()
    {
        var entity = new PersonDto { Id = 1, Name = "Jacob", Age = 30 };

        DeepAssert.Equivalent(new { Id = 99, Name = "Jacob", Age = 30 }, entity, o => o.Ignore(p => p.Id));
    }

    [Fact]
    public void Passes_when_both_are_null()
    {
        DeepAssert.Equivalent<object?, PersonDto?>(null, null);
    }
}
