using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Mwh.Sample.Common.Extension
{


    [TestClass()]
    public class AjaxDictionaryTests
    {

        [TestMethod()]
        public void AddTest()
        {
            var myTest = new AjaxDictionary<int, string>();
            myTest.Add(1, "test");
            Assert.AreEqual(1, myTest.GetList().Count);
        }

        [TestMethod()]
        public void AddTest1()
        {
            var myTest = new AjaxDictionary<int, string>();
            myTest.Add(1, "test");
            Assert.AreEqual("test", myTest[1]);
        }
        [TestMethod()]
        public void AjaxDictionaryTest()
        {
            var myTest = new AjaxDictionary<int, string>();
            Assert.AreNotEqual(null, myTest);
        }

        [TestMethod()]
        public void AjaxDictionaryTest1()
        {
            var myTest = new AjaxDictionary<int, string>();
            Assert.AreNotEqual(null, myTest);
        }

        [TestMethod()]
        public void GetListTest()
        {
            var myTest = new AjaxDictionary<int, string>();
            myTest.Add(1, "one");
            myTest.Add(2, "two");
            var myResult = myTest.GetList().FirstOrDefault();
            Assert.AreEqual("1 - one", myResult);
        }

        [TestMethod()]
        public void GetObjectDataTest() { }
    }
}
