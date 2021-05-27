// ***********************************************************************
// Assembly         : Mwh.Sample.Common.Tests
// Author           : mark
// Created          : 04-04-2020
//
// Last Modified By : mark
// Last Modified On : 04-04-2020
// ***********************************************************************
// <copyright file="AjaxDictionaryTests.cs" company="Mwh.Sample.Common.Tests">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Mwh.Sample.Common.Extension
{
    /// <summary>
    /// Defines test class AjaxDictionaryTests.
    /// </summary>
    [TestClass()]
    public class AjaxDictionaryTests
    {

        /// <summary>
        /// Defines the test method AddTest.
        /// </summary>
        [TestMethod()]
        public void AddTest()
        {
            var myTest = new AjaxDictionary<int, string>();
            myTest.Add(1, "test");
            Assert.AreEqual(1, myTest.GetList().Count);
        }

        /// <summary>
        /// Defines the test method AddTest1.
        /// </summary>
        [TestMethod()]
        public void AddTest1()
        {
            var myTest = new AjaxDictionary<int, string>();
            myTest.Add(1, "test");
            Assert.AreEqual("test", myTest[1]);
        }
        /// <summary>
        /// Defines the test method AjaxDictionaryTest.
        /// </summary>
        [TestMethod()]
        public void AjaxDictionaryTest()
        {
            var myTest = new AjaxDictionary<int, string>();
            Assert.AreNotEqual(null, myTest);
        }

        /// <summary>
        /// Defines the test method AjaxDictionaryTest1.
        /// </summary>
        [TestMethod()]
        public void AjaxDictionaryTest1()
        {
            var myTest = new AjaxDictionary<int, string>();
            Assert.AreNotEqual(null, myTest);
        }

        /// <summary>
        /// Defines the test method GetListTest.
        /// </summary>
        [TestMethod()]
        public void GetListTest()
        {
            var myTest = new AjaxDictionary<int, string>();
            myTest.Add(1, "one");
            myTest.Add(2, "two");
            var myResult = myTest.GetList().FirstOrDefault();
            Assert.AreEqual("1 - one", myResult);
        }

        /// <summary>
        /// Defines the test method GetObjectDataTest.
        /// </summary>
        [TestMethod()]
        public void GetObjectDataTest() { }
    }
}
