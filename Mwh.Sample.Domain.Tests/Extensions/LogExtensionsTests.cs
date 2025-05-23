﻿using System.Xml;

namespace Mwh.Sample.Domain.Tests.Extensions;

[TestClass]
public class LogExtensionsTests
{

    // Helper method to validate XML
    private bool IsValidXml(string xmlString)
    {
        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    [TestMethod]
    public void GetSerializeObjectString_SerializeListOfObjects_ReturnsValidXmlString()
    {
        // Arrange
        List<Person> people = new List<Person>
        {
            new Person { Name = "John Doe", Age = 30 },
            new Person { Name = "Jane Smith", Age = 25 },
            new Person { Name = null, Age = 15 }
        };

        // Act
        string result = people.GetSerializeObjectString();

        // Assert
        Assert.IsTrue(IsValidXml(result));
    }
    [TestMethod]
    public void GetSerializeObjectString_SerializeSingleObject_ReturnsValidXmlString()
    {
        // Arrange
        Person person = new Person { Name = "John Doe", Age = 30 };

        // Act
        string result = person.GetSerializeObjectString();

        // Assert
        Assert.IsTrue(IsValidXml(result));
    }
    [TestMethod]
    public void GetSerializeObjectString_StateUnderTest_ExpectedBehavior()
    {
        // Arrange
        EmployeeDto objectToSerialize = default;

        // Act
        string result = LogExtensions.GetSerializeObjectString(objectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetSerializeObjectString_StateUnderTest_ExpectedBehavior1()
    {
        // Arrange
        List<EmployeeDto> lstObjectToSerialize = new();

        // Act
        string result = LogExtensions.GetSerializeObjectString(lstObjectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetSerializeObjectString_StateUnderTest_ExpectedBehaviorNull()
    {
        // Arrange
        EmployeeDto objectToSerialize = null;

        // Act
        string result = LogExtensions.GetSerializeObjectString(objectToSerialize);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void IsSimpleType_ComplexType_ReturnsFalse()
    {
        // Arrange
        Type complexType = typeof(Person);

        // Act
        bool result = complexType.IsSimpleType();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSimpleType_KnownSimpleTypes_ReturnsTrue()
    {
        // Arrange
        Type[] simpleTypes = new Type[]
        {
            typeof(int),
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
        };

        // Act & Assert
        foreach (Type type in simpleTypes)
        {
            Assert.IsTrue(type.IsSimpleType());
        }
    }

    [TestMethod]
    public void IsSimpleType_StateUnderTest_ExpectedBehaviorFalse()
    {
        // Arrange
        EmployeeDto type = new(99, "Test", 33, "Test", "Test", EmployeeDepartmentEnum.IT);

        // Act
        bool result = LogExtensions.IsSimpleType(type.GetType());

        // Assert
        Assert.IsNotNull(result);
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void IsSimpleType_StateUnderTest_ExpectedBehaviorTrue()
    {
        // Arrange
        string type = "test";

        // Act
        bool result = LogExtensions.IsSimpleType(type.GetType());

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result);
    }
}


// Test data class
public class Person
{
    public int Age { get; set; }
    public string Name { get; set; } = string.Empty;
}

