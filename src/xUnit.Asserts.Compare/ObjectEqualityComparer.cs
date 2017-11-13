using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using KellermanSoftware.CompareNetObjects;

namespace Xunit.Asserts.Compare {
    public class ObjectEqualityComparer<T> : IEqualityComparer<T> {
        private readonly CompareLogic compareLogic;

        public ObjectEqualityComparer (params string[] propertiesToIgnore) {
            compareLogic = new CompareLogic ();
            compareLogic.Config.MembersToIgnore = new List<string> ();

            foreach (var property in propertiesToIgnore) {
                compareLogic.Config.MembersToIgnore.Add (property);
            }
        }

        public bool Equals (T x, T y) {
            return compareLogic.Compare (x, y).AreEqual;
        }

        public int GetHashCode (T obj) {
            return obj.GetHashCode ();
        }
    }
}