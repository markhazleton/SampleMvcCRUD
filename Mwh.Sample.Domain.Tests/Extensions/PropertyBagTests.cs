using System.Runtime.Serialization;

namespace Mwh.Sample.Domain.Tests.Extensions;

/// <summary>
/// Defines test class PropertyBagTests.
/// </summary>
[TestClass()]
public class PropertyBagTests
{
    /// <summary>
    /// Defines the test method AddTest.
    /// </summary>
    [TestMethod()]
    public void AddTest()
    {
        var myTest = new PropertyBag<int, string>();
        myTest.Add(1, "test");
        Assert.AreEqual("1:test", myTest.ToString());
        Assert.AreEqual(1, myTest.GetList().Count);
    }
    /// <summary>
    /// Defines the test method AddTest.
    /// </summary>
    [TestMethod()]
    public void AddTest_Null()
    {
        string? nullString = null;
        var myTest = new PropertyBag<int, string>();
        myTest.Add(1, nullString);
        Assert.AreEqual(string.Empty, myTest.ToString());
    }

    /// <summary>
    /// Defines the test method AddTest1.
    /// </summary>
    [TestMethod()]
    public void AddTest_Duplicate()
    {
        var myTest = new PropertyBag<int, string>();
        myTest.Add(1, "test");
        myTest[2] = "test";
        myTest.Add(3, "initial");

        var tempDic = new Dictionary<int, string>();
        tempDic.Add(3, "test3");
        tempDic.Add(4, "test4");
        myTest.Add(tempDic);

        myTest.Add(1, "test1");
        myTest[2] = "test2";


        var onestring = myTest.ToString();

        Assert.AreEqual("1:test1, 2:test2, 3:test3, 4:test4", onestring);
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
        var myTest = new PropertyBag<int, string>();
        Assert.AreNotEqual(null, myTest);
        Assert.AreEqual(myTest.ToString(), string.Empty);
    }

    /// <summary>
    /// Defines the test method AjaxDictionaryTest1.
    /// </summary>
    [TestMethod()]
    public void AjaxDictionaryTest1()
    {
        var myTest = new PropertyBag<int, string>();
        Assert.AreNotEqual(null, myTest);
    }

    /// <summary>
    /// Defines the test method GetListTest.
    /// </summary>
    [TestMethod()]
    public void GetListTest()
    {
        var myTest = new PropertyBag<int, string>();
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

    [TestMethod]
    public void GetObjectData_WithNonEmptyPropertyBag_SerializesDictionaryToSerializationInfo()
    {
        // Arrange
        var propertyBag = new PropertyBag<string, int>();
        propertyBag.Add("Key1", 1);
        propertyBag.Add("Key2", 2);
        var serializationInfo = new SerializationInfo(typeof(PropertyBag<string, int>), new FormatterConverter());

        // Act
        propertyBag.GetObjectData(serializationInfo);

        // Assert
        Assert.AreEqual(2, serializationInfo.MemberCount);
        Assert.AreEqual(1, serializationInfo.GetValue("Key1", typeof(int)));
        Assert.AreEqual(2, serializationInfo.GetValue("Key2", typeof(int)));
    }

    [TestMethod]
    public void GetObjectData_WithEmptyPropertyBag_DoesNotAddValuesToSerializationInfo()
    {
        // Arrange
        var propertyBag = new PropertyBag<string, int>();
        var serializationInfo = new SerializationInfo(typeof(PropertyBag<string, int>), new FormatterConverter());

        // Act
        propertyBag.GetObjectData(serializationInfo);

        // Assert
        Assert.AreEqual(0, serializationInfo.MemberCount);
    }
}
