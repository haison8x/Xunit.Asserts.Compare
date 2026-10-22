using System.Linq.Expressions;
using Xunit.Asserts.Compare.Internal;

namespace Xunit.Asserts.Compare.Tests;

public class MemberPathTests
{
    private static string[] Names(Expression<Func<Person, object?>> expression) =>
        MemberPath.GetMembers(expression).Select(m => m.Name).ToArray();

    [Fact]
    public void Returns_a_single_member()
    {
        Assert.Equal(["Name"], Names(p => p.Name));
    }

    [Fact]
    public void Unwraps_boxing_of_value_types()
    {
        Assert.Equal(["Age"], Names(p => p.Age));
    }

    [Fact]
    public void Returns_nested_members_outermost_first()
    {
        Assert.Equal(["Address", "City"], Names(p => p.Address!.City));
    }

    [Fact]
    public void Unwraps_casts_inside_the_path()
    {
        Assert.Equal(["Address", "City"], Names(p => ((Address)p.Address!).City));
        Assert.Equal(["Address", "City"], Names(p => (p.Address as Address)!.City));
    }

    [Fact]
    public void GetLeafMember_returns_the_last_member()
    {
        Assert.Equal("City", MemberPath.GetLeafMember((Expression<Func<Person, object?>>)(p => p.Address!.City)).Name);
    }

    [Fact]
    public void Rejects_method_calls()
    {
        Assert.Throws<ArgumentException>(() => Names(p => p.Orders.Count()));
    }

    [Fact]
    public void Rejects_members_not_rooted_in_the_parameter()
    {
        Assert.Throws<ArgumentException>(() => Names(_ => DateTime.Now));
    }

    [Fact]
    public void Rejects_constants()
    {
        Assert.Throws<ArgumentException>(() => Names(_ => 42));
    }

    [Fact]
    public void Rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => MemberPath.GetMembers(null!));
    }
}
