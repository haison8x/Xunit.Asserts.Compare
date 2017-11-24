using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using KellermanSoftware.CompareNetObjects;
using Xunit;

namespace Xunit.Asserts.Compare {
    public static class DeepAssert {

        public static void Equal<T> (T expected, T actual, params string[] propertiesToIgnore) {
            var compareLogic = new CompareLogic ();
            compareLogic.Config.MembersToIgnore = new List<string> ();

            foreach (var property in propertiesToIgnore) {
                compareLogic.Config.MembersToIgnore.Add (property);
            }

            var comparisonResult = compareLogic.Compare (expected, actual);

            if (!comparisonResult.AreEqual) {
                throw new ObjectEqualException (expected, actual, comparisonResult.DifferencesString);
            }
        }
    }
}