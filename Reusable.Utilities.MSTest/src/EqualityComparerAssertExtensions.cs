﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;

namespace Reusable.Utilities.MSTest
{
    public interface IEqualityComparerAssertExtensions { }

    public static class EqualityComparerAssertExtensions
    {
        public static void IsCanonical<T>(this IEqualityComparerAssertExtensions assert, IEqualityComparer<T> comparer, T other)
        {
            Assert.IsTrue(comparer.Equals(default, default), CreateMessage("null == null"));
            Assert.IsFalse(comparer.Equals(default, other), CreateMessage("null != other"));
            Assert.IsFalse(comparer.Equals(other, default), CreateMessage("other != null"));
            Assert.IsTrue(comparer.Equals(other, other), CreateMessage("other == other"));

            string CreateMessage(string requirement)
            {
                return $"{typeof(IEqualityComparer<T>).ToPrettyString()} violates the {requirement.QuoteWith("'")} requirement.";
            }
        }
    }
}