using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using ReservoirVisualisationProject.Extensions;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace ReservoirVisualisationProject.Tests
{
    [TestFixture]
    public class MathExtensionMethodsTests
    {
        [Test]
        public void IsNumeric_NumericTypes_IsTrue([Values(typeof(sbyte), typeof(byte), typeof(short), 
                                                                    typeof(ushort), typeof(int), typeof(uint), 
                                                                    typeof(long), typeof(ulong), typeof(float), 
                                                                    typeof(double), typeof(decimal))] Type type)
        {
            object value = Convert.ChangeType(1, type);

            Assert.IsTrue(value.IsNumeric());
        }

        [Test]
        public void IsNumeric_NonNumericTypes_IsFalse([Values(typeof(string), typeof(char), typeof(bool))] Type type)
        {
            object value = Convert.ChangeType(1, type);

            Assert.IsFalse(value.IsNumeric());
        }
    }
}
