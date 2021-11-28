namespace Mwh.Sample.Domain.Tests.Extension;

/// <summary>
/// Defines test class StringExtensionsTests.
/// </summary>
[TestClass()]
public class StringExtensionsTests
{
    /// <summary>
    /// Defines the test method GetDecimalFromStringTest.
    /// </summary>
    [TestMethod()]
    public void GetDecimalFromStringTest()
    {
        string myTest = "1.99";
        decimal myDefault = 2.99M;

        var myValue = myTest.GetDecimalFromString(myDefault);
        Assert.AreEqual((decimal)1.99, myValue);
    }

    /// <summary>
    /// Defines the test method GetDecimalFromStringTestInvalidString.
    /// </summary>
    [TestMethod()]
    public void GetDecimalFromStringTestInvalidString()
    {
        string myTest = "INVALID";
        decimal myDefault = 2.99M;
        var myValue = myTest.GetDecimalFromString(myDefault);
        Assert.AreEqual(myDefault, myValue);
    }

    /// <summary>
    /// Defines the test method GetDecimalFromStringTestNullString.
    /// </summary>
    [TestMethod()]
    public void GetDecimalFromStringTestNullString()
    {
        string myTest = null;
        decimal myDefault = 2.99M;
        var myValue = myTest.GetDecimalFromString(myDefault);
        Assert.AreEqual(myDefault, myValue);
    }

    /// <summary>
    /// Defines the test method GetIntFromStringTest.
    /// </summary>
    [TestMethod()]
    public void GetIntFromStringTest()
    {
        string myTest = "1";
        int? myDefault = 0;

        var myValue = myTest.GetIntFromString(myDefault);
        Assert.AreEqual(1, myValue);
    }

    /// <summary>
    /// Defines the test method GetIntFromStringTestInvalidString.
    /// </summary>
    [TestMethod()]
    public void GetIntFromStringTestInvalidString()
    {
        string myTest = "INVALID";
        int? myDefault = 0;
        var myValue = myTest.GetIntFromString(myDefault);
        Assert.AreEqual(myDefault, myValue);
    }

    /// <summary>
    /// Defines the test method GetIntFromStringTestNullDefault.
    /// </summary>
    [TestMethod()]
    public void GetIntFromStringTestNullDefault()
    {
        string myTest = null;
        int? myDefault = null;
        var myValue = myTest.GetIntFromString(myDefault);
        Assert.AreEqual(myDefault, myValue);
    }

    /// <summary>
    /// Defines the test method GetIntFromStringTestNullString.
    /// </summary>
    [TestMethod()]
    public void GetIntFromStringTestNullString()
    {
        string myTest = null;
        int? myDefault = 0;
        var myValue = myTest.GetIntFromString(myDefault);
        Assert.AreEqual(myDefault, myValue);
    }

    /// <summary>
    /// Defines the test method GetNullIntFromStringTest.
    /// </summary>
    [TestMethod()]
    public void GetNullIntFromStringTest()
    {
        string myTest = "1";
        var myValue = myTest.GetIntFromString(0);
        Assert.AreEqual(1, myValue);
    }

    /// <summary>
    /// Defines the test method GetNullIntFromStringTestInvalidString.
    /// </summary>
    [TestMethod()]
    public void GetNullIntFromStringTestInvalidString()
    {
        string myTest = "INVALID";
        var myValue = myTest.GetIntFromString(0);
        Assert.AreEqual(0, myValue);
    }

    /// <summary>
    /// Defines the test method GetNullIntFromStringTestNullString.
    /// </summary>
    [TestMethod()]
    public void GetNullIntFromStringTestNullString()
    {
        string myTest = null;
        var myValue = myTest.GetIntFromString(0);
        Assert.AreEqual(0, myValue);
    }

    /// <summary>
    /// Defines the test method IndexOfNthTest.
    /// </summary>
    [TestMethod()]
    public void IndexOfNthTest()
    {
        string myTest = "0123456789";
        Assert.AreEqual(3, myTest.IndexOfNth("3", 1));
    }

    /// <summary>
    /// Defines the test method IndexOfNthTestMultipleFind.
    /// </summary>
    [TestMethod()]
    public void IndexOfNthTestMultipleFind()
    {
        string myTest = "333333";
        Assert.AreEqual(2, myTest.IndexOfNth("3", 3));
    }

    /// <summary>
    /// Defines the test method IndexOfNthTestSourceNull.
    /// </summary>
    [TestMethod()]
    public void IndexOfNthTestSourceNull()
    {
        string myTest = null;
        Assert.AreEqual(0, myTest.IndexOfNth("3", 3));
    }

    /// <summary>
    /// Defines the test method IndexOfNthTestValueNegativeNth.
    /// </summary>
    [TestMethod()]
    [ExpectedException(typeof(ArgumentException))]
    public void IndexOfNthTestValueNegativeNth()
    {
        string myTest = "333333";
        var RunTest = myTest.IndexOfNth("3", -1);
    }

    /// <summary>
    /// Defines the test method IndexOfNthTestValueNull.
    /// </summary>
    [TestMethod()]
    public void IndexOfNthTestValueNull()
    {
        string myTest = "333333";
        Assert.AreEqual(0, myTest.IndexOfNth(null, 3));
    }

    /// <summary>
    /// Defines the test method LeftTest.
    /// </summary>
    [TestMethod()]
    public void LeftTest()
    {
        string myTest = "0123456789";
        Assert.AreEqual("01", myTest.Left(2));
    }

    /// <summary>
    /// Defines the test method LeftTestNullShortString.
    /// </summary>
    [TestMethod()]
    public void LeftTestNullShortString()
    {
        string myTest = "01";
        Assert.AreEqual("01", myTest.Left(8));
    }

    /// <summary>
    /// Defines the test method LeftTestNullString.
    /// </summary>
    [TestMethod()]
    public void LeftTestNullString()
    {
        string myTest = null;
        Assert.AreEqual(string.Empty, myTest.Left(2));
    }

    /// <summary>
    /// Defines the test method RightTest.
    /// </summary>
    [TestMethod()]
    public void RightTest()
    {
        string myTest = "0123456789";
        Assert.AreEqual("89", myTest.Right(2));
    }

    /// <summary>
    /// Defines the test method RightTestNullString.
    /// </summary>
    [TestMethod()]
    public void RightTestNullString()
    {
        string myTest = null;
        Assert.AreEqual(string.Empty, myTest.Right(2));
    }

    /// <summary>
    /// Defines the test method RightTestToShort.
    /// </summary>
    [TestMethod()]
    public void RightTestToShort()
    {
        string myTest = "01";
        Assert.AreEqual("01", myTest.Right(8));
    }

    /// <summary>
    /// Defines the test method TrimIfNotNullTest.
    /// </summary>
    [TestMethod()]
    public void TrimIfNotNullTest()
    {
        string myTest = "   MyTest   ";
        Assert.AreEqual("MyTest", myTest.TrimIfNotNull());
    }

    /// <summary>
    /// Defines the test method TrimIfNotNullTestWithNull.
    /// </summary>
    [TestMethod()]
    public void TrimIfNotNullTestWithNull()
    {
        string myTest = null;
        Assert.AreEqual(string.Empty, myTest.TrimIfNotNull());
    }
}
