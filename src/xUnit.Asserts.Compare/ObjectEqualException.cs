using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using KellermanSoftware.CompareNetObjects;
using Xunit.Sdk;

namespace Xunit.Asserts.Compare
{
    public class ObjectEqualException : AssertActualExpectedException
    {
       private readonly string message;

        public ObjectEqualException(object expected, object actual, string message)
            : base(expected, actual, "Assert.Equal() Failure")
        {
            this.message = message;
        }

        public override string Message => message;
    }
}