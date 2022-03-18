
using System.Runtime.Serialization;

namespace Mwh.Sample.Common.Tests.Extension;
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
    public void AddTest_Duplicate()
    {
        var myTest = new AjaxDictionary<int, string>();
        myTest.Add(1, "test");
        myTest[2] = "test";
        myTest.Add(3, "initial");

        var tempDic = new Dictionary<int, string>();
        tempDic.Add(3, "test3");
        tempDic.Add(4, "test4");
        myTest.Add(tempDic);

        myTest.Add(1, "test1");
        myTest[2] = "test2";

        Assert.AreEqual("test1", myTest[1]);
        Assert.AreEqual("test2", myTest[2]);
        Assert.AreEqual("test3", myTest[3]);
        Assert.AreEqual("test4", myTest[4]);
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
}
