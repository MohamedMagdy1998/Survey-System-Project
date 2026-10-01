using Domain.Common.Const;
using Xunit;

namespace Domain.UnitTests.Common.Const;

public class MyRateLimitOptionsTests
{
    [Fact]
    public void MyRateLimit_WhenAccessed_ReturnsExpectedSectionName()
    {
        // Act & Assert
        Assert.Equal("MyRateLimit", MyRateLimitOptions.MyRateLimit);
    }

    [Fact]
    public void Constructor_DefaultValues_SetsDefaultLimits()
    {
        // Act
        var options = new MyRateLimitOptions();

        // Assert
        Assert.Equal(1000, options.PermitLimit);
        Assert.Equal(100, options.QueueLimit);
    }

    [Fact]
    public void PermitLimit_WhenAssigned_StoresCorrectValue()
    {
        // Arrange
        var options = new MyRateLimitOptions();
        const int expectedPermitLimit = 50;

        // Act
        options.PermitLimit = expectedPermitLimit;

        // Assert
        Assert.Equal(expectedPermitLimit, options.PermitLimit);
    }

    [Fact]
    public void QueueLimit_WhenAssigned_StoresCorrectValue()
    {
        // Arrange
        var options = new MyRateLimitOptions();
        const int expectedQueueLimit = 10;

        // Act
        options.QueueLimit = expectedQueueLimit;

        // Assert
        Assert.Equal(expectedQueueLimit, options.QueueLimit);
    }
}
