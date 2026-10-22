namespace Xunit.Asserts.Compare.Tests;

public class CollectionTests
{
    [Fact]
    public void IgnoreCollectionOrder_passes_for_reordered_primitives()
    {
        int[] expected = [1, 2, 3];
        int[] actual = [3, 1, 2];

        DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder());
    }

    [Fact]
    public void IgnoreCollectionOrder_passes_for_reordered_objects()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Orders.Reverse();

        DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder());
    }

    [Fact]
    public void IgnoreCollectionOrder_still_fails_for_different_items()
    {
        int[] expected = [1, 2, 3];
        int[] actual = [3, 1, 4];

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder()));
    }

    [Fact]
    public void IgnoreCollectionOrder_still_fails_for_different_counts()
    {
        int[] expected = [1, 2, 3];
        int[] actual = [1, 2];

        Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.IgnoreCollectionOrder()));
    }

    [Fact]
    public void MatchCollectionItemsBy_pairs_items_by_key_regardless_of_order()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Orders.Reverse();

        DeepAssert.Equal(expected, actual, o => o.MatchCollectionItemsBy<Order>(x => x.Id));
    }

    [Fact]
    public void MatchCollectionItemsBy_reports_member_differences_of_matched_items()
    {
        var expected = TestData.CreatePerson();
        var actual = TestData.CreatePerson();
        actual.Orders.Reverse();
        actual.Orders[0].Price = 2m; // order 11 (Pen)

        var ex = Assert.Throws<EqualException>(() => DeepAssert.Equal(expected, actual, o => o.MatchCollectionItemsBy<Order>(x => x.Id)));

        Assert.Contains("Price", ex.Message);
    }

    [Fact]
    public void MatchCollectionItemsBy_supports_composite_keys()
    {
        List<Order> expected = [new() { Id = 1, Product = "A", Price = 1 }, new() { Id = 1, Product = "B", Price = 2 }];
        List<Order> actual = [new() { Id = 1, Product = "B", Price = 2 }, new() { Id = 1, Product = "A", Price = 1 }];

        DeepAssert.Equal(expected, actual, o => o.MatchCollectionItemsBy<Order>(x => x.Id, x => x.Product));
    }

    [Fact]
    public void MatchCollectionItemsBy_rejects_nested_keys()
    {
        Assert.Throws<ArgumentException>(() => new DeepAssertOptions<Person>().MatchCollectionItemsBy<Person>(p => p.Address!.City));
    }

    [Fact]
    public void MatchCollectionItemsBy_requires_at_least_one_key()
    {
        Assert.Throws<ArgumentException>(() => new DeepAssertOptions<Person>().MatchCollectionItemsBy<Order>());
    }
}
