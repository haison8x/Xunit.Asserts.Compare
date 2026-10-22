namespace Xunit.Asserts.Compare.Tests;

public class IgnoreTests
{
    [Fact]
    public void Ignore_by_expression_skips_top_level_member()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";

        DeepAssert.Equal(expected, actual, o => o.Ignore(p => p.Name));
    }

    [Fact]
    public void Ignore_by_expression_handles_boxed_value_types()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Age = 99;
        actual.CreatedAt = DateTime.UtcNow;

        DeepAssert.Equal(expected, actual, o => o.Ignore(p => p.Age, p => p.CreatedAt));
    }

    [Fact]
    public void Ignore_by_expression_skips_nested_member()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Address!.City = "Paris";

        DeepAssert.Equal(expected, actual, o => o.Ignore(p => p.Address!.City));
    }

    [Fact]
    public void Ignore_nested_member_does_not_ignore_siblings()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Address!.Street = "2 Main St";

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.Ignore(p => p.Address!.City)));
    }

    [Fact]
    public void Ignore_by_expression_is_scoped_to_the_declaring_type()
    {
        // Person.Id is ignored, but Order.Id is still compared.
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Id = 2;
        actual.Orders[0].Id = 99;

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.Ignore(p => p.Id)));

        Assert.Contains("1 difference", ex.Message);
        Assert.Contains("Orders[0].Id", ex.Message);
    }

    [Fact]
    public void Ignore_by_expression_works_for_inherited_members()
    {
        var expected = new Employee { Id = 1, Name = "A", Department = "IT" };
        var actual = new Employee { Id = 1, Name = "B", Department = "IT" };

        DeepAssert.Equal(expected, actual, o => o.Ignore(e => e.Name));
    }

    [Fact]
    public void Ignore_by_name_skips_member_everywhere()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Id = 2;
        actual.Orders[0].Id = 99;

        DeepAssert.Equal(expected, actual, o => o.Ignore("Id"));
    }

    [Fact]
    public void Ignore_by_type_qualified_name()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Orders[0].Id = 99;

        DeepAssert.Equal(expected, actual, o => o.Ignore("Order.Id"));
    }

    [Fact]
    public void Ignore_by_wildcard_name()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.CreatedAt = DateTime.UtcNow;

        DeepAssert.Equal(expected, actual, o => o.Ignore("*At"));
    }

    [Fact]
    public void Ignore_can_be_called_multiple_times()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";
        actual.Age = 99;

        DeepAssert.Equal(expected, actual, o => o.Ignore(p => p.Name).Ignore("Age"));
    }

    [Fact]
    public void IgnoreType_skips_every_instance_of_the_type()
    {
        var expected = new Document { Title = "Spec", Audit = new AuditInfo { ModifiedBy = "alice" } };
        var actual = new Document { Title = "Spec", Audit = new AuditInfo { ModifiedBy = "bob" } };

        DeepAssert.Equal(expected, actual, o => o.IgnoreType<AuditInfo>());
    }

    [Fact]
    public void CompareOnly_by_expression_ignores_everything_else()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";
        actual.Orders.Clear();

        DeepAssert.Equal(expected, actual, o => o.CompareOnly(p => p.Id, p => p.Age));
    }

    [Fact]
    public void CompareOnly_still_fails_on_included_members()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Age = 31;

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.CompareOnly(p => p.Id, p => p.Age)));
    }

    [Fact]
    public void CompareOnly_with_nested_expression_includes_the_whole_path()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";
        actual.Address!.Street = "2 Main St";

        DeepAssert.Equal(expected, actual, o => o.CompareOnly(p => p.Address!.City));

        actual.Address.City = "Paris";
        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.CompareOnly(p => p.Address!.City)));
    }

    [Fact]
    public void CompareOnly_by_name()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Name = "James";

        DeepAssert.Equal(expected, actual, o => o.CompareOnly("Id", "Age"));
    }

    [Fact]
    public void Ignore_rejects_expressions_that_are_not_member_access()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => DeepAssert.Equal(TestData.CreatePerson(), TestData.CreatePerson(), o => o.Ignore(p => p.Name!.ToUpper())));

        Assert.Contains("member access", ex.Message);
    }

    [Fact]
    public void Ignore_rejects_the_parameter_itself()
    {
        Assert.Throws<ArgumentException>(
            () => DeepAssert.Equal(TestData.CreatePerson(), TestData.CreatePerson(), o => o.Ignore(p => p)));
    }

    [Fact]
    public void Ignore_rejects_null_and_blank_names()
    {
        Assert.Throws<ArgumentException>(() => new DeepAssertOptions<Person>().Ignore(" "));
        Assert.Throws<ArgumentNullException>(() => new DeepAssertOptions<Person>().Ignore((string[])null!));
    }
}
