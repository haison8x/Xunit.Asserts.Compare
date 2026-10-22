using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using Xunit.Asserts.Compare.Internal;
using Xunit.Sdk;

namespace Xunit.Asserts.Compare;

/// <summary>
/// xUnit.net assertions that compare whole object graphs member by member, using CompareNETObjects.
/// </summary>
/// <remarks>
/// Unlike <c>Assert.Equal</c>, these assertions do not rely on <see cref="object.Equals(object)"/>:
/// two objects are equal when all their public properties and fields are equal, recursively,
/// including collections and dictionaries. Failure messages list every differing member path.
/// </remarks>
public static class DeepAssert
{
    /// <summary>
    /// Verifies that two objects are deeply equal.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The value to check.</param>
    /// <param name="propertiesToIgnore">
    /// Member names to skip. A name can be plain (<c>"Id"</c>), type-qualified (<c>"Order.Id"</c>) or a wildcard (<c>"*At"</c>).
    /// </param>
    /// <exception cref="EqualException">The objects differ.</exception>
    public static void Equal<T>(T expected, T actual, params string[] propertiesToIgnore)
    {
        AssertEqual(expected, actual, LegacyOptions<T>(propertiesToIgnore));
    }

    /// <summary>
    /// Verifies that two objects are deeply equal, using the given options.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The value to check.</param>
    /// <param name="configure">Configures the comparison, for example <c>o =&gt; o.Ignore(x =&gt; x.Id).IgnoreCollectionOrder()</c>.</param>
    /// <exception cref="EqualException">The objects differ.</exception>
    public static void Equal<T>(T expected, T actual, Action<DeepAssertOptions<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        AssertEqual(expected, actual, CreateOptions(configure));
    }

    /// <summary>
    /// Verifies that two objects are not deeply equal.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="expected">The value that <paramref name="actual"/> must differ from.</param>
    /// <param name="actual">The value to check.</param>
    /// <param name="propertiesToIgnore">Member names to skip; see <see cref="Equal{T}(T, T, string[])"/>.</param>
    /// <exception cref="NotEqualException">The objects are deeply equal.</exception>
    public static void NotEqual<T>(T expected, T actual, params string[] propertiesToIgnore)
    {
        AssertNotEqual(expected, actual, LegacyOptions<T>(propertiesToIgnore));
    }

    /// <summary>
    /// Verifies that two objects are not deeply equal, using the given options.
    /// </summary>
    /// <typeparam name="T">The type of the objects to compare.</typeparam>
    /// <param name="expected">The value that <paramref name="actual"/> must differ from.</param>
    /// <param name="actual">The value to check.</param>
    /// <param name="configure">Configures the comparison.</param>
    /// <exception cref="NotEqualException">The objects are deeply equal.</exception>
    public static void NotEqual<T>(T expected, T actual, Action<DeepAssertOptions<T>> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        AssertNotEqual(expected, actual, CreateOptions(configure));
    }

    /// <summary>
    /// Verifies that the members of <paramref name="expected"/> are deeply equal to the members with the same
    /// names on <paramref name="actual"/>, even when the two have different types — for example an anonymous
    /// object against an entity, or a DTO against a domain model.
    /// </summary>
    /// <remarks>
    /// The expected object drives the comparison: members that exist only on <paramref name="actual"/> are not
    /// checked, so an anonymous object can list just the members you care about. Every member of
    /// <paramref name="expected"/> must exist on <paramref name="actual"/>, which catches typos in the expectation;
    /// call <see cref="DeepAssertOptions{T}.IgnoreMissingMembers"/> to relax that.
    /// </remarks>
    /// <typeparam name="TExpected">The type of the expected value, often an anonymous type.</typeparam>
    /// <typeparam name="TActual">The type of the value to check. Member expressions in <paramref name="configure"/> are written against this type.</typeparam>
    /// <param name="expected">The expected value.</param>
    /// <param name="actual">The value to check.</param>
    /// <param name="configure">Optionally configures the comparison.</param>
    /// <exception cref="EqualException">A member differs, or a member of <paramref name="expected"/> does not exist on <paramref name="actual"/>.</exception>
    public static void Equivalent<TExpected, TActual>(TExpected expected, TActual actual, Action<DeepAssertOptions<TActual>>? configure = null)
    {
        var options = new DeepAssertOptions<TActual>().IgnoreObjectTypes();
        configure?.Invoke(options);
        var config = options.BuildConfig();

        if (!options.AllowMissingMembers)
        {
            var missing = MissingMemberFinder.Find(expected, actual, config);
            if (missing.Count > 0)
            {
                throw EqualException.ForMismatchedValues(Format(expected), Format(actual), DescribeMissingMembers(missing, actual));
            }
        }

        AssertEqual(expected, actual, config);
    }

    /// <summary>
    /// Verifies that a collection contains at least one item deeply equal to <paramref name="expected"/>.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="expected">The item to look for.</param>
    /// <param name="collection">The collection to search. It is enumerated once.</param>
    /// <param name="configure">Optionally configures how items are compared.</param>
    /// <exception cref="ContainsException">No item is deeply equal to <paramref name="expected"/>.</exception>
    public static void Contains<T>(T expected, IEnumerable<T> collection, Action<DeepAssertOptions<T>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var items = collection.ToList();

        if (IndexOf(expected, items, configure) < 0)
        {
            using var tracker = items.AsTracker();
            throw ContainsException.ForCollectionItemNotFound(Format(expected), tracker.FormatStart());
        }
    }

    /// <summary>
    /// Verifies that a collection contains no item deeply equal to <paramref name="expected"/>.
    /// </summary>
    /// <typeparam name="T">The item type.</typeparam>
    /// <param name="expected">The item that must not be present.</param>
    /// <param name="collection">The collection to search. It is enumerated once.</param>
    /// <param name="configure">Optionally configures how items are compared.</param>
    /// <exception cref="DoesNotContainException">An item is deeply equal to <paramref name="expected"/>.</exception>
    public static void DoesNotContain<T>(T expected, IEnumerable<T> collection, Action<DeepAssertOptions<T>>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var items = collection.ToList();

        var index = IndexOf(expected, items, configure);
        if (index >= 0)
        {
            using var tracker = items.AsTracker();
            var formattedCollection = tracker.FormatIndexedMismatch(index, out var pointerIndent);
            throw DoesNotContainException.ForCollectionItemFound(Format(expected), index, pointerIndent, formattedCollection);
        }
    }

    private static DeepAssertOptions<T> CreateOptions<T>(Action<DeepAssertOptions<T>>? configure)
    {
        var options = new DeepAssertOptions<T>();
        configure?.Invoke(options);
        return options;
    }

    private static DeepAssertOptions<T> LegacyOptions<T>(string[]? propertiesToIgnore)
    {
        var options = new DeepAssertOptions<T>();
        if (propertiesToIgnore is { Length: > 0 })
        {
            options.Ignore(propertiesToIgnore);
        }

        return options;
    }

    private static void AssertEqual<T>(object? expected, object? actual, DeepAssertOptions<T> options)
    {
        AssertEqual(expected, actual, options.BuildConfig());
    }

    private static void AssertEqual(object? expected, object? actual, ComparisonConfig config)
    {
        var result = new CompareLogic(config).Compare(expected, actual);
        if (!result.AreEqual)
        {
            throw EqualException.ForMismatchedValues(Format(expected), Format(actual), DescribeDifferences(result));
        }
    }

    private static void AssertNotEqual<T>(object? expected, object? actual, DeepAssertOptions<T> options)
    {
        var result = new CompareLogic(options.BuildConfig()).Compare(expected, actual);
        if (result.AreEqual)
        {
            throw NotEqualException.ForEqualValues(Format(expected), Format(actual), "Objects are deeply equal");
        }
    }

    private static int IndexOf<T>(T expected, List<T> items, Action<DeepAssertOptions<T>>? configure)
    {
        var config = CreateOptions(configure).BuildConfig();
        // Only a yes/no answer is needed per item, so stop at the first difference.
        config.MaxDifferences = 1;
        var compareLogic = new CompareLogic(config);

        return items.FindIndex(item => compareLogic.Compare(expected, item).AreEqual);
    }

    private static string DescribeDifferences(ComparisonResult result)
    {
        var count = result.Differences.Count;
        var builder = new StringBuilder()
            .Append("Deep comparison found ")
            .Append(count)
            .Append(count == 1 ? " difference" : " differences");

        if (result.ExceededDifferences)
        {
            var max = result.Config.MaxDifferences;
            builder.Append(" (stopped after ").Append(max).Append(max == 1 ? " difference" : " differences")
                .Append("; call MaxDifferences(n) to report more)");
        }

        foreach (var difference in result.Differences)
        {
            builder.AppendLine().Append("  - ").Append(difference);
        }

        return builder.ToString();
    }

    private static string DescribeMissingMembers(IReadOnlyList<string> missing, object? actual)
    {
        var builder = new StringBuilder()
            .Append(missing.Count)
            .Append(missing.Count == 1 ? " expected member does not exist on " : " expected members do not exist on ")
            .Append(actual is null ? "the actual value" : ArgumentFormatter.FormatTypeName(actual.GetType()))
            .Append(" (call IgnoreMissingMembers() to skip them)");

        foreach (var path in missing)
        {
            builder.AppendLine().Append("  - ").Append(path);
        }

        return builder.ToString();
    }

    private static string Format(object? value) => ArgumentFormatter.Format(value);
}
