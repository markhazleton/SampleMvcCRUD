namespace Mwh.Sample.Domain.Tests.Models;

[TestClass]
public class PagingParameterModelTests
{
    [TestMethod]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        PagingParameterModel paging = new PagingParameterModel();

        // Assert
        Assert.AreEqual(300, paging.PageSize);
        Assert.AreEqual(1, paging.PageNumber);
    }

    [TestMethod]
    public void PageSize_ShouldNotExceedMaxPageSize()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel();

        // Act
        paging.PageSize = 10000;

        // Assert
        Assert.AreEqual(5000, paging.PageSize);
    }

    [TestMethod]
    public void PageSize_ShouldAcceptValueBelowMaxPageSize()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel();

        // Act
        paging.PageSize = 100;

        // Assert
        Assert.AreEqual(100, paging.PageSize);
    }

    [TestMethod]
    public void PageSize_ShouldAcceptMaxPageSize()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel();

        // Act
        paging.PageSize = 5000;

        // Assert
        Assert.AreEqual(5000, paging.PageSize);
    }

    [TestMethod]
    public void GetMetaData_ShouldReturnCorrectMetadataForFirstPage()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel
        {
            PageNumber = 1,
            PageSize = 10
        };
        long totalCount = 100;

        // Act
        object metadata = paging.GetMetaData(totalCount);

        // Assert
        Assert.IsNotNull(metadata);
        var metadataType = metadata.GetType();
        Assert.AreEqual(100L, metadataType.GetProperty("totalCount")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("pageSize")?.GetValue(metadata));
        Assert.AreEqual(1, metadataType.GetProperty("currentPage")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("totalPages")?.GetValue(metadata));
        Assert.AreEqual("No", metadataType.GetProperty("previousPage")?.GetValue(metadata));
        Assert.AreEqual("Yes", metadataType.GetProperty("nextPage")?.GetValue(metadata));
    }

    [TestMethod]
    public void GetMetaData_ShouldReturnCorrectMetadataForMiddlePage()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel
        {
            PageNumber = 5,
            PageSize = 10
        };
        long totalCount = 100;

        // Act
        object metadata = paging.GetMetaData(totalCount);

        // Assert
        Assert.IsNotNull(metadata);
        var metadataType = metadata.GetType();
        Assert.AreEqual(100L, metadataType.GetProperty("totalCount")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("pageSize")?.GetValue(metadata));
        Assert.AreEqual(5, metadataType.GetProperty("currentPage")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("totalPages")?.GetValue(metadata));
        Assert.AreEqual("Yes", metadataType.GetProperty("previousPage")?.GetValue(metadata));
        Assert.AreEqual("Yes", metadataType.GetProperty("nextPage")?.GetValue(metadata));
    }

    [TestMethod]
    public void GetMetaData_ShouldReturnCorrectMetadataForLastPage()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel
        {
            PageNumber = 10,
            PageSize = 10
        };
        long totalCount = 100;

        // Act
        object metadata = paging.GetMetaData(totalCount);

        // Assert
        Assert.IsNotNull(metadata);
        var metadataType = metadata.GetType();
        Assert.AreEqual(100L, metadataType.GetProperty("totalCount")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("pageSize")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("currentPage")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("totalPages")?.GetValue(metadata));
        Assert.AreEqual("Yes", metadataType.GetProperty("previousPage")?.GetValue(metadata));
        Assert.AreEqual("No", metadataType.GetProperty("nextPage")?.GetValue(metadata));
    }

    [TestMethod]
    public void GetMetaData_ShouldHandleSinglePage()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel
        {
            PageNumber = 1,
            PageSize = 100
        };
        long totalCount = 50;

        // Act
        object metadata = paging.GetMetaData(totalCount);

        // Assert
        Assert.IsNotNull(metadata);
        var metadataType = metadata.GetType();
        Assert.AreEqual(50L, metadataType.GetProperty("totalCount")?.GetValue(metadata));
        Assert.AreEqual(100, metadataType.GetProperty("pageSize")?.GetValue(metadata));
        Assert.AreEqual(1, metadataType.GetProperty("currentPage")?.GetValue(metadata));
        Assert.AreEqual(1, metadataType.GetProperty("totalPages")?.GetValue(metadata));
        Assert.AreEqual("No", metadataType.GetProperty("previousPage")?.GetValue(metadata));
        Assert.AreEqual("No", metadataType.GetProperty("nextPage")?.GetValue(metadata));
    }

    [TestMethod]
    public void GetMetaData_ShouldHandleZeroTotalCount()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel
        {
            PageNumber = 1,
            PageSize = 10
        };
        long totalCount = 0;

        // Act
        object metadata = paging.GetMetaData(totalCount);

        // Assert
        Assert.IsNotNull(metadata);
        var metadataType = metadata.GetType();
        Assert.AreEqual(0L, metadataType.GetProperty("totalCount")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("pageSize")?.GetValue(metadata));
        Assert.AreEqual(1, metadataType.GetProperty("currentPage")?.GetValue(metadata));
        Assert.AreEqual(0, metadataType.GetProperty("totalPages")?.GetValue(metadata));
        Assert.AreEqual("No", metadataType.GetProperty("previousPage")?.GetValue(metadata));
        Assert.AreEqual("No", metadataType.GetProperty("nextPage")?.GetValue(metadata));
    }

    [TestMethod]
    public void GetMetaData_ShouldCalculateTotalPagesCorrectlyWithRemainder()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel
        {
            PageNumber = 1,
            PageSize = 10
        };
        long totalCount = 95;

        // Act
        object metadata = paging.GetMetaData(totalCount);

        // Assert
        Assert.IsNotNull(metadata);
        var metadataType = metadata.GetType();
        Assert.AreEqual(95L, metadataType.GetProperty("totalCount")?.GetValue(metadata));
        Assert.AreEqual(10, metadataType.GetProperty("totalPages")?.GetValue(metadata));
    }

    [TestMethod]
    public void PageNumber_ShouldBeSettable()
    {
        // Arrange
        PagingParameterModel paging = new PagingParameterModel();

        // Act
        paging.PageNumber = 5;

        // Assert
        Assert.AreEqual(5, paging.PageNumber);
    }
}
