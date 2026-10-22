using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Xunit.Asserts.Compare.Internal;

namespace Xunit.Asserts.Compare;

/// <summary>
/// Fluent options that control how <see cref="DeepAssert"/> compares two object graphs.
/// </summary>
/// <typeparam name="T">The type of the objects being compared. Member expressions are written against this type.</typeparam>
/// <remarks>
/// Every method returns the same instance so calls can be chained:
/// <code>
/// DeepAssert.Equal(expected, actual, o =&gt; o
///     .Ignore(x =&gt; x.Id, x =&gt; x.CreatedAt)
///     .IgnoreCollectionOrder()
///     .WithDoubleTolerance(0.001));
/// </code>
/// </remarks>
public sealed class DeepAssertOptions<T>
{
    /// <summary>
    /// The number of differences reported in a failure message unless <see cref="MaxDifferences(int)"/> is called.
    /// </summary>
    public const int DefaultMaxDifferences = 100;

    private readonly List<Action<ComparisonConfig>> _steps = [];
    private readonly List<MemberInfo> _ignoredMembers = [];
    private readonly List<Action<ComparisonConfig>> _configureCallbacks = [];

    // CompareNETObjects always skips members missing on the actual side (IgnoreMissingProperties defaults to true),
    // so the strict check lives in DeepAssert.Equivalent and this flag turns it off.
    internal bool AllowMissingMembers { get; private set; }

    /// <summary>
    /// Ignores the given members. Each expression must be a member access on the parameter,
    /// for example <c>x =&gt; x.Name</c> or <c>x =&gt; x.Address.City</c>.
    /// </summary>
    /// <remarks>
    /// The last member of the path is ignored wherever its declaring type appears in the graph,
    /// so <c>x =&gt; x.Address.City</c> ignores <c>City</c> on every <c>Address</c>, while
    /// <c>City</c> on other types is still compared.
    /// </remarks>
    /// <param name="members">Member-access expressions of the members to ignore.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> Ignore(params Expression<Func<T, object?>>[] members)
    {
        ArgumentNullException.ThrowIfNull(members);
        foreach (var member in members)
        {
            _ignoredMembers.Add(MemberPath.GetLeafMember(member));
        }

        return this;
    }

    /// <summary>
    /// Ignores members by name. A name can be a plain member name (<c>"Id"</c>, ignored on every type),
    /// a type-qualified name (<c>"Order.Id"</c>) or a wildcard (<c>"*At"</c>, <c>"Audit*"</c>).
    /// Names are case sensitive.
    /// </summary>
    /// <param name="memberNames">The member names to ignore.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> Ignore(params string[] memberNames)
    {
        ValidateNames(memberNames);
        _steps.Add(c => c.MembersToIgnore.AddRange(memberNames));
        return this;
    }

    /// <summary>
    /// Ignores every value of type <typeparamref name="TIgnore"/> anywhere in the graph.
    /// </summary>
    /// <typeparam name="TIgnore">The class type to skip.</typeparam>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> IgnoreType<TIgnore>()
    {
        _steps.Add(c => c.ClassTypesToIgnore.Add(typeof(TIgnore)));
        return this;
    }

    /// <summary>
    /// Compares only the given members and ignores all others.
    /// For a nested path such as <c>x =&gt; x.Address.City</c>, every member on the path is included.
    /// </summary>
    /// <remarks>
    /// Inclusion works by member name at every level of the graph: including <c>Address</c>
    /// alone does not include the members of <c>Address</c>. Name each nested member you want compared.
    /// </remarks>
    /// <param name="members">Member-access expressions of the members to compare.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> CompareOnly(params Expression<Func<T, object?>>[] members)
    {
        ArgumentNullException.ThrowIfNull(members);
        var names = members.SelectMany(MemberPath.GetMembers).Select(m => m.Name).Distinct().ToList();
        _steps.Add(c => c.MembersToInclude.AddRange(names));
        return this;
    }

    /// <summary>
    /// Compares only the members with the given names, at every level of the graph, and ignores all others.
    /// </summary>
    /// <param name="memberNames">The member names to compare. Case sensitive.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> CompareOnly(params string[] memberNames)
    {
        ValidateNames(memberNames);
        _steps.Add(c => c.MembersToInclude.AddRange(memberNames));
        return this;
    }

    /// <summary>
    /// Treats collections as equal when they contain the same items in any order.
    /// </summary>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> IgnoreCollectionOrder()
    {
        _steps.Add(c => c.IgnoreCollectionOrder = true);
        return this;
    }

    /// <summary>
    /// Ignores collection order and pairs items of type <typeparamref name="TItem"/> by the given key members,
    /// then reports member-level differences between paired items (instead of "item not found").
    /// </summary>
    /// <typeparam name="TItem">The collection item type.</typeparam>
    /// <param name="keys">One or more direct members of <typeparamref name="TItem"/> that identify an item, for example <c>x =&gt; x.Id</c>.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> MatchCollectionItemsBy<TItem>(params Expression<Func<TItem, object?>>[] keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        if (keys.Length == 0)
        {
            throw new ArgumentException("At least one key member is required.", nameof(keys));
        }

        var names = keys.Select(key =>
        {
            var members = MemberPath.GetMembers(key);
            if (members.Count != 1)
            {
                throw new ArgumentException($"Key '{key}' must be a direct member of {typeof(TItem).Name}, such as 'x => x.Id'.", nameof(keys));
            }

            return members[0].Name;
        }).ToList();

        _steps.Add(c =>
        {
            c.IgnoreCollectionOrder = true;
            c.CollectionMatchingSpec ??= new Dictionary<Type, IEnumerable<string>>();
            c.CollectionMatchingSpec[typeof(TItem)] = names;
        });
        return this;
    }

    /// <summary>
    /// Treats <see cref="double"/> and <see cref="float"/> values as equal when they differ by no more than <paramref name="tolerance"/>.
    /// </summary>
    /// <param name="tolerance">The largest accepted absolute difference. Must be zero or positive.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> WithDoubleTolerance(double tolerance)
    {
        if (double.IsNaN(tolerance) || double.IsInfinity(tolerance) || tolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, "Tolerance must be a finite, non-negative number.");
        }

        _steps.Add(c => c.DoublePrecision = tolerance);
        return this;
    }

    /// <summary>
    /// Treats <see cref="decimal"/> values as equal when they differ by no more than <paramref name="tolerance"/>.
    /// </summary>
    /// <param name="tolerance">The largest accepted absolute difference. Must be zero or positive.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> WithDecimalTolerance(decimal tolerance)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tolerance);
        _steps.Add(c => c.DecimalPrecision = tolerance);
        return this;
    }

    /// <summary>
    /// Treats <see cref="DateTime"/> and <see cref="DateTimeOffset"/> values as equal when they differ by no more than <paramref name="tolerance"/>.
    /// The comparison has millisecond resolution.
    /// </summary>
    /// <param name="tolerance">The largest accepted difference. Must be zero or positive.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> WithDateTimeTolerance(TimeSpan tolerance)
    {
        if (tolerance < TimeSpan.Zero || tolerance.TotalMilliseconds > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(tolerance), tolerance, $"Tolerance must be between zero and {int.MaxValue} milliseconds.");
        }

        var milliseconds = (int)tolerance.TotalMilliseconds;
        _steps.Add(c => c.MaxMillisecondsDateDifference = milliseconds);
        return this;
    }

    /// <summary>
    /// Compares strings case-insensitively.
    /// </summary>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> IgnoreCase()
    {
        _steps.Add(c => c.CaseSensitive = false);
        return this;
    }

    /// <summary>
    /// Treats a <see langword="null"/> string and an empty string as equal.
    /// </summary>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> TreatNullAndEmptyStringAsEqual()
    {
        _steps.Add(c => c.TreatStringEmptyAndNullTheSame = true);
        return this;
    }

    /// <summary>
    /// Ignores leading and trailing whitespace when comparing strings.
    /// </summary>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> IgnoreLeadingTrailingWhitespace()
    {
        _steps.Add(c => c.IgnoreStringLeadingTrailingWhitespace = true);
        return this;
    }

    /// <summary>
    /// Compares objects member by member even when their runtime types differ
    /// (for example an anonymous object against an entity). <see cref="DeepAssert.Equivalent{TExpected, TActual}"/> turns this on automatically.
    /// </summary>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> IgnoreObjectTypes()
    {
        _steps.Add(c => c.IgnoreObjectTypes = true);
        return this;
    }

    /// <summary>
    /// With <see cref="DeepAssert.Equivalent{TExpected, TActual}"/>, stops failing when a member of the expected
    /// object does not exist on the actual object; such members are skipped instead.
    /// </summary>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> IgnoreMissingMembers()
    {
        AllowMissingMembers = true;
        return this;
    }

    /// <summary>
    /// Also compares private properties and private fields.
    /// </summary>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> IncludePrivateMembers()
    {
        _steps.Add(c =>
        {
            c.ComparePrivateProperties = true;
            c.ComparePrivateFields = true;
        });
        return this;
    }

    /// <summary>
    /// Uses <paramref name="comparer"/> to decide whether two values of type <typeparamref name="TValue"/> are equal,
    /// wherever they appear in the graph. Two <see langword="null"/> values are equal; a <see langword="null"/>
    /// and a non-null value are not, so the delegate is only called with non-null values.
    /// </summary>
    /// <typeparam name="TValue">The type the comparer applies to (matched exactly, not by inheritance).</typeparam>
    /// <param name="comparer">Returns <see langword="true"/> when the two values should be treated as equal.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> WithComparer<TValue>(Func<TValue, TValue, bool> comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        _steps.Add(c => c.CustomComparers.Add(new CustomComparer<TValue, TValue>(
            (left, right) => left is null || right is null ? left is null && right is null : comparer(left, right))));
        return this;
    }

    /// <summary>
    /// Sets how many differences are collected before the comparison stops. Defaults to <see cref="DefaultMaxDifferences"/>.
    /// </summary>
    /// <param name="maxDifferences">The maximum number of differences. Must be at least 1.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> MaxDifferences(int maxDifferences)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxDifferences, 1);
        _steps.Add(c => c.MaxDifferences = maxDifferences);
        return this;
    }

    /// <summary>
    /// Escape hatch: changes the underlying CompareNETObjects <see cref="ComparisonConfig"/> directly.
    /// Callbacks run after all other options, in the order they were added.
    /// </summary>
    /// <param name="configure">A callback that edits the configuration.</param>
    /// <returns>This instance.</returns>
    public DeepAssertOptions<T> Configure(Action<ComparisonConfig> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _configureCallbacks.Add(configure);
        return this;
    }

    /// <summary>
    /// Builds a fresh <see cref="ComparisonConfig"/> from the options recorded so far.
    /// </summary>
    internal ComparisonConfig BuildConfig()
    {
        var config = new ComparisonConfig { MaxDifferences = DefaultMaxDifferences };

        foreach (var step in _steps)
        {
            step(config);
        }

        foreach (var member in _ignoredMembers)
        {
            // When types are ignored the expected side usually has a different declaring type
            // (e.g. an anonymous object), so a type-qualified name would never match it.
            config.MembersToIgnore.Add(config.IgnoreObjectTypes || member.DeclaringType is null
                ? member.Name
                : $"{member.DeclaringType.Name}.{member.Name}");
        }

        foreach (var callback in _configureCallbacks)
        {
            callback(config);
        }

        return config;
    }

    private static void ValidateNames(string[] memberNames)
    {
        ArgumentNullException.ThrowIfNull(memberNames);
        foreach (var name in memberNames)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(memberNames));
        }
    }
}
