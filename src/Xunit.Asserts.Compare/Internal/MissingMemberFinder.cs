using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using KellermanSoftware.CompareNetObjects;

namespace Xunit.Asserts.Compare.Internal;

/// <summary>
/// Finds members of the expected graph that have no counterpart (by name) on the actual graph.
/// </summary>
/// <remarks>
/// When object types are ignored, CompareNETObjects walks the members of the expected object and silently
/// skips any member the actual object lacks. That makes a typo in an anonymous expectation pass unnoticed,
/// so <see cref="DeepAssert.Equivalent{TExpected, TActual}"/> runs this check first.
/// </remarks>
internal static class MissingMemberFinder
{
    private const int MaxDepth = 64;

    public static IReadOnlyList<string> Find(object? expected, object? actual, ComparisonConfig config)
    {
        var missing = new List<string>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        Walk(expected, actual, string.Empty, config, missing, visited, depth: 0);
        return missing;
    }

    private static void Walk(object? expected, object? actual, string path, ComparisonConfig config, List<string> missing, HashSet<object> visited, int depth)
    {
        if (expected is null || actual is null || depth > MaxDepth || IsSimple(expected.GetType()))
        {
            return;
        }

        if (!expected.GetType().IsValueType && !visited.Add(expected))
        {
            return;
        }

        if (expected is IDictionary expectedDictionary && actual is IDictionary actualDictionary)
        {
            foreach (DictionaryEntry entry in expectedDictionary)
            {
                if (actualDictionary.Contains(entry.Key))
                {
                    Walk(entry.Value, actualDictionary[entry.Key], $"{path}[{entry.Key}]", config, missing, visited, depth + 1);
                }
            }

            return;
        }

        if (expected is IEnumerable expectedItems && actual is IEnumerable actualItems)
        {
            var index = 0;
            var actualEnumerator = actualItems.GetEnumerator();
            foreach (var item in expectedItems)
            {
                if (!actualEnumerator.MoveNext())
                {
                    break;
                }

                Walk(item, actualEnumerator.Current, $"{path}[{index++}]", config, missing, visited, depth + 1);
            }

            return;
        }

        var expectedType = expected.GetType();
        var actualType = actual.GetType();

        foreach (var member in GetMembers(expectedType, config))
        {
            if (IsExcluded(config, member, expectedType))
            {
                continue;
            }

            var memberPath = path.Length == 0 ? member.Name : $"{path}.{member.Name}";
            var counterpart = GetMembers(actualType, config).FirstOrDefault(m => m.Name == member.Name);

            if (counterpart is null)
            {
                missing.Add(memberPath);
                continue;
            }

            if (TryGetValue(member, expected, out var expectedValue) && TryGetValue(counterpart, actual, out var actualValue))
            {
                Walk(expectedValue, actualValue, memberPath, config, missing, visited, depth + 1);
            }
        }
    }

    // Mirrors CompareNETObjects' ExcludeLogic, which cannot be called directly because it relies on
    // lookup sets that CompareLogic only fills in internally.
    private static bool IsExcluded(ComparisonConfig config, MemberInfo member, Type objectType)
    {
        if (config.MembersToInclude.Count > 0 && !config.MembersToInclude.Contains(member.Name))
        {
            return true;
        }

        var memberType = member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;
        if (config.ClassTypesToIgnore.Contains(memberType) || config.TypesToIgnore.Contains(memberType))
        {
            return true;
        }

        return config.MembersToIgnore.Any(pattern =>
            pattern == member.Name
            || pattern == $"{objectType.Name}.{member.Name}"
            || (member.DeclaringType is not null && pattern == $"{member.DeclaringType.Name}.{member.Name}")
            || MatchesWildcard(pattern, member.Name));
    }

    private static bool MatchesWildcard(string pattern, string name)
    {
        var startsWithStar = pattern.StartsWith('*');
        var endsWithStar = pattern.EndsWith('*');

        if (startsWithStar && endsWithStar && pattern.Length > 2)
        {
            return name.Contains(pattern[1..^1], StringComparison.Ordinal);
        }

        if (startsWithStar && pattern.Length >= 2)
        {
            return name.EndsWith(pattern[1..], StringComparison.Ordinal);
        }

        if (endsWithStar && pattern.Length >= 2)
        {
            return name.StartsWith(pattern[..^1], StringComparison.Ordinal);
        }

        return false;
    }

    private static IEnumerable<MemberInfo> GetMembers(Type type, ComparisonConfig config)
    {
        var flags = BindingFlags.Instance | BindingFlags.Public;
        if (config.ComparePrivateProperties || config.ComparePrivateFields)
        {
            flags |= BindingFlags.NonPublic;
        }

        var properties = config.CompareProperties
            ? type.GetProperties(flags).Where(p => p.CanRead && p.GetIndexParameters().Length == 0 && (p.GetMethod!.IsPublic || config.ComparePrivateProperties))
            : [];

        var fields = config.CompareFields
            ? type.GetFields(flags).Where(f => !f.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)) && (f.IsPublic || config.ComparePrivateFields))
            : [];

        return properties.Cast<MemberInfo>().Concat(fields);
    }

    private static bool TryGetValue(MemberInfo member, object target, out object? value)
    {
        try
        {
            value = member switch
            {
                PropertyInfo property => property.GetValue(target),
                FieldInfo field => field.GetValue(target),
                _ => null,
            };
            return true;
        }
        catch (TargetInvocationException)
        {
            value = null;
            return false;
        }
    }

    private static bool IsSimple(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(DateOnly)
            || type == typeof(TimeOnly)
            || type == typeof(TimeSpan)
            || type == typeof(Guid)
            || type == typeof(Uri)
            || typeof(Type).IsAssignableFrom(type);
    }
}
