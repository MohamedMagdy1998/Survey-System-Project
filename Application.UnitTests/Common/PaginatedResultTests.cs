using Application.Common;
using Xunit;

namespace Application.UnitTests.Common;

public class PaginatedResultTests
{
    [Fact]
    public void Constructor_WhenEvenDivision_CalculatesTotalPagesCorrectly()
    {
        // Arrange
        var items = new List<string> { "item1", "item2" };

        // Act
        var result = new PaginatedResult<string>(items, count: 20, pageNumber: 1, pageSize: 10);

        // Assert
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(20, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(10, result.PageSize);
        Assert.Same(items, result.Items);
    }

    [Fact]
    public void Constructor_WhenOddDivision_RoundsUpTotalPages()
    {
        // Arrange
        var items = new List<int> { 1, 2, 3 };

        // Act
        var result = new PaginatedResult<int>(items, count: 25, pageNumber: 2, pageSize: 10);

        // Assert
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public void Constructor_WhenZeroCount_SetsTotalPagesToZero()
    {
        // Arrange
        var items = new List<string>();

        // Act
        var result = new PaginatedResult<string>(items, count: 0, pageNumber: 1, pageSize: 10);

        // Assert
        Assert.Equal(0, result.TotalPages);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public void HasPreviousPage_WhenPageNumberIsFirstPage_ReturnsFalse()
    {
        // Arrange
        var result = new PaginatedResult<string>(new List<string>(), count: 10, pageNumber: 1, pageSize: 5);

        // Act & Assert
        Assert.False(result.HasPreviousPage);
    }

    [Fact]
    public void HasPreviousPage_WhenPageNumberGreaterThanOne_ReturnsTrue()
    {
        // Arrange
        var result = new PaginatedResult<string>(new List<string>(), count: 20, pageNumber: 2, pageSize: 5);

        // Act & Assert
        Assert.True(result.HasPreviousPage);
    }

    [Fact]
    public void HasNextPage_WhenPageNumberEqualsTotalPages_ReturnsFalse()
    {
        // Arrange
        var result = new PaginatedResult<string>(new List<string>(), count: 20, pageNumber: 2, pageSize: 10);

        // Act & Assert
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public void HasNextPage_WhenPageNumberLessThanTotalPages_ReturnsTrue()
    {
        // Arrange
        var result = new PaginatedResult<string>(new List<string>(), count: 30, pageNumber: 2, pageSize: 10);

        // Act & Assert
        Assert.True(result.HasNextPage);
    }
}
