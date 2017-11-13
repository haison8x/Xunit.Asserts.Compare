using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using KellermanSoftware.CompareNetObjects;
using Xunit;

namespace Xunit.Asserts.Compare {
    public static class DeepAssert {

        public static void Equal<T> (T expected, T actual, params string[] propertiesToIgnore) {
            var comparer = new ObjectEqualityComparer<T> (propertiesToIgnore);
            Assert.Equal<T> (expected, actual, comparer);
        }
    }
}