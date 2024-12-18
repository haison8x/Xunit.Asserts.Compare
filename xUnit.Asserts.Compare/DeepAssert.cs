using KellermanSoftware.CompareNetObjects;
using Xunit.Sdk;

namespace Xunit.Asserts.Compare;

public static class DeepAssert {

    public static void Equal<T> (T expected, T actual, params string[] propertiesToIgnore) {
        var compareLogic = new CompareLogic
        {
            Config =
            {
                MembersToIgnore = []
            }
        };

        foreach (var property in propertiesToIgnore) {
            compareLogic.Config.MembersToIgnore.Add (property);
        }

        var comparisonResult = compareLogic.Compare (expected, actual);

        if (!comparisonResult.AreEqual) {
            throw EqualException.ForMismatchedValues(expected, actual, comparisonResult.DifferencesString);
        }
    }

    public static void NotEqual<T>(T expected, T actual, params string[] propertiesToIgnore)
    {
        var compareLogic = new CompareLogic
        {
            Config =
            {
                MembersToIgnore = []
            }
        };

        foreach (var property in propertiesToIgnore)
        {
            compareLogic.Config.MembersToIgnore.Add(property);
        }

        var comparisonResult = compareLogic.Compare(expected, actual);

        if (comparisonResult.AreEqual)
        {
            throw NotEqualException.ForEqualValues("expected", "actual", comparisonResult.DifferencesString);
        }
    }
}