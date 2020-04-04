using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Mwh.Sample.Common.Extension
{
    [TestClass()]
    public class StringExtensionsTests
    {

        [TestMethod()]
        public void GetDecimalFromStringTest()
        {
            string myTest = "1.99";
            decimal myDefault = 2.99M;

            var myValue = myTest.GetDecimalFromString(myDefault);
            Assert.AreEqual((decimal)1.99, myValue);
        }

        [TestMethod()]
        public void GetDecimalFromStringTestInvalidString()
        {
            string myTest = "INVALID";
            decimal myDefault = 2.99M;
            var myValue = myTest.GetDecimalFromString(myDefault);
            Assert.AreEqual(myDefault, myValue);
        }

        [TestMethod()]
        public void GetDecimalFromStringTestNullString()
        {
            string myTest = null;
            decimal myDefault = 2.99M;
            var myValue = myTest.GetDecimalFromString(myDefault);
            Assert.AreEqual(myDefault, myValue);
        }

        [TestMethod()]
        public void GetIntFromStringTest()
        {
            string myTest = "1";
            int? myDefault = 0;

            var myValue = myTest.GetIntFromString(myDefault);
            Assert.AreEqual(1, myValue);
        }

        [TestMethod()]
        public void GetIntFromStringTestInvalidString()
        {
            string myTest = "INVALID";
            int? myDefault = 0;
            var myValue = myTest.GetIntFromString(myDefault);
            Assert.AreEqual(myDefault, myValue);
        }

        [TestMethod()]
        public void GetIntFromStringTestNullDefault()
        {
            string myTest = null;
            int? myDefault = null;
            var myValue = myTest.GetIntFromString(myDefault);
            Assert.AreEqual(myDefault, myValue);
        }

        [TestMethod()]
        public void GetIntFromStringTestNullString()
        {
            string myTest = null;
            int? myDefault = 0;
            var myValue = myTest.GetIntFromString(myDefault);
            Assert.AreEqual(myDefault, myValue);
        }

        [TestMethod()]
        public void GetNullIntFromStringTest()
        {
            string myTest = "1";
            var myValue = myTest.GetIntFromString(0);
            Assert.AreEqual(1, myValue);
        }

        [TestMethod()]
        public void GetNullIntFromStringTestInvalidString()
        {
            string myTest = "INVALID";
            var myValue = myTest.GetIntFromString(0);
            Assert.AreEqual(0, myValue);
        }

        [TestMethod()]
        public void GetNullIntFromStringTestNullString()
        {
            string myTest = null;
            var myValue = myTest.GetIntFromString(0);
            Assert.AreEqual(0, myValue);
        }

        [TestMethod()]
        public void IndexOfNthTest()
        {
            string myTest = "0123456789";
            Assert.AreEqual(3, myTest.IndexOfNth("3", 1));
        }

        [TestMethod()]
        public void IndexOfNthTestMultipleFind()
        {
            string myTest = "333333";
            Assert.AreEqual(2, myTest.IndexOfNth("3", 3));
        }

        [TestMethod()]
        public void IndexOfNthTestSourceNull()
        {
            string myTest = null;
            Assert.AreEqual(0, myTest.IndexOfNth("3", 3));
        }

        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void IndexOfNthTestValueNegativeNth()
        {
            string myTest = "333333";
            var RunTest = myTest.IndexOfNth("3", -1);
        }

        [TestMethod()]
        public void IndexOfNthTestValueNull()
        {
            string myTest = "333333";
            Assert.AreEqual(0, myTest.IndexOfNth(null, 3));
        }

        [TestMethod()]
        public void LeftTest()
        {
            string myTest = "0123456789";
            Assert.AreEqual("01", myTest.Left(2));
        }

        [TestMethod()]
        public void LeftTestNullShortString()
        {
            string myTest = "01";
            Assert.AreEqual("01", myTest.Left(8));
        }

        [TestMethod()]
        public void LeftTestNullString()
        {
            string myTest = null;
            Assert.AreEqual(string.Empty, myTest.Left(2));
        }

        [TestMethod()]
        public void RightTest()
        {
            string myTest = "0123456789";
            Assert.AreEqual("89", myTest.Right(2));
        }

        [TestMethod()]
        public void RightTestNullString()
        {
            string myTest = null;
            Assert.AreEqual(string.Empty, myTest.Right(2));
        }

        [TestMethod()]
        public void RightTestToShort()
        {
            string myTest = "01";
            Assert.AreEqual("01", myTest.Right(8));
        }
        [TestMethod()]
        public void TrimIfNotNullTest()
        {
            string myTest = "   MyTest   ";
            Assert.AreEqual("MyTest", myTest.TrimIfNotNull());
        }

        [TestMethod()]
        public void TrimIfNotNullTestWithNull()
        {
            string myTest = null;
            Assert.AreEqual(string.Empty, myTest.TrimIfNotNull());
        }
    }
}
