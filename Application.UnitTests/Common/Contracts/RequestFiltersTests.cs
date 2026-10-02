using Application.Common.Contracts;
using Xunit;

namespace Application.UnitTests.Common.Contracts;

public class RequestFiltersTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesExpectedDefaults()
    {
        // Act
        var filters = new RequestFilters();

        // Assert
        Assert.Equal(1, filters.PageNumber);
        Assert.Equal(10, filters.PageSize);
        Assert.Null(filters.SearchValue);
        Assert.Null(filters.SortColumn);
        Assert.Equal("Asc", filters.SortDirection);
    }

    [Fact]
    public void PageSize_WhenAssignedGreaterThanMax_CapsAtMaxPageSize()
    {
        // Act
        var filters = new RequestFilters { PageSize = 100 };

        // Assert
        Assert.Equal(50, filters.PageSize);
    }

    [Fact]
    public void PageSize_WhenAssignedLessThanOrEqualToMax_StoresAssignedValue()
    {
        // Act
        var filters = new RequestFilters { PageSize = 25 };

        // Assert
        Assert.Equal(25, filters.PageSize);
    }

    [Fact]
    public void Properties_WhenAssignedCustomValues_StoresValuesCorrectly()
    {
        // Act
        var filters = new RequestFilters
        {
            PageNumber = 3,
            PageSize = 20,
            SearchValue = "keyword",
            SortColumn = "Title",
            SortDirection = "Desc"
        };

        // Assert
        Assert.Equal(3, filters.PageNumber);
        Assert.Equal(20, filters.PageSize);
        Assert.Equal("keyword", filters.SearchValue);
        Assert.Equal("Title", filters.SortColumn);
        Assert.Equal("Desc", filters.SortDirection);
    }
}
