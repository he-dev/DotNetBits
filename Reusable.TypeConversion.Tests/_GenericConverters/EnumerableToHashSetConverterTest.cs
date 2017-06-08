using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class EnumerableToHashSetConverterTest : ConverterTest
    {
        [TestMethod]
        public void Convert_StringArray_HashSetInt32()
        {
            var result =
                TypeConverter.Empty
                    .Add<StringToInt32Converter>()
                    .Add<EnumerableToHashSetConverter>()
                    .Convert(new[] { "3", "7" }, typeof(HashSet<int>)) as HashSet<int>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result.Verify().Contains(3).Contains(7);
        }

        [TestMethod]
        public void Convert_Int32Array_HashSetString()
        {
            var result =
                TypeConverter.Empty
                    .Add<Int32ToStringConverter>()
                    .Add<EnumerableToHashSetConverter>()
                    .Convert(new[] { 3, 7 }, typeof(HashSet<string>)) as HashSet<string>;

            result.Verify().IsNotNull();
            result.Count.Verify().IsEqual(2);
            result.Verify().Contains("3").Contains("7");
        }
    }
}