namespace Xunit.Asserts.Compare.Tests;

public class ContainsTests
{
    private static List<Order> Orders() => TestData.CreatePerson().Orders;

    [Fact]
    public void Contains_passes_when_a_deeply_equal_item_exists()
    {
        DeepAssert.Contains(new Order { Id = 11, Product = "Pen", Price = 1.20m, Weight = 0.01 }, Orders());
    }

    [Fact]
    public void Contains_fails_when_no_item_is_deeply_equal()
    {
        var ex = Assert.Throws<ContainsException>(
            () => DeepAssert.Contains(new Order { Id = 11, Product = "Pen", Price = 9.99m, Weight = 0.01 }, Orders()));

        Assert.Contains("Assert.Contains() Failure", ex.Message);
    }

    [Fact]
    public void Contains_fails_for_an_empty_collection()
    {
        Assert.Throws<ContainsException>(() => DeepAssert.Contains(new Order(), new List<Order>()));
    }

    [Fact]
    public void Contains_honours_options()
    {
        DeepAssert.Contains(new Order { Id = 999, Product = "Pen", Price = 1.20m, Weight = 0.01 }, Orders(), o => o.Ignore(x => x.Id));
    }

    [Fact]
    public void Contains_rejects_a_null_collection()
    {
        Assert.Throws<ArgumentNullException>(() => DeepAssert.Contains(new Order(), (IEnumerable<Order>)null!));
    }

    [Fact]
    public void DoesNotContain_passes_when_no_item_is_deeply_equal()
    {
        DeepAssert.DoesNotContain(new Order { Id = 11, Product = "Pen", Price = 9.99m, Weight = 0.01 }, Orders());
    }

    [Fact]
    public void DoesNotContain_fails_when_a_deeply_equal_item_exists()
    {
        var ex = Assert.Throws<DoesNotContainException>(
            () => DeepAssert.DoesNotContain(new Order { Id = 11, Product = "Pen", Price = 1.20m, Weight = 0.01 }, Orders()));

        Assert.Contains("Assert.DoesNotContain() Failure", ex.Message);
        Assert.Contains("(pos 1)", ex.Message);
    }

    [Fact]
    public void DoesNotContain_honours_options()
    {
        Assert.Throws<DoesNotContainException>(
            () => DeepAssert.DoesNotContain(new Order { Id = 999, Product = "Pen", Price = 1.20m, Weight = 0.01 }, Orders(), o => o.Ignore(x => x.Id)));
    }

    [Fact]
    public void DoesNotContain_rejects_a_null_collection()
    {
        Assert.Throws<ArgumentNullException>(() => DeepAssert.DoesNotContain(new Order(), (IEnumerable<Order>)null!));
    }
}
