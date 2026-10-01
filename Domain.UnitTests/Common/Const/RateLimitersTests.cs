using Domain.Common.Const;
using Xunit;

namespace Domain.UnitTests.Common.Const;

public class RateLimitersTests
{
    [Fact]
    public void IpLimiter_WhenAccessed_ReturnsExpectedValue()
    {
        // Act & Assert
        Assert.Equal("ipLimit", RateLimiters.IpLimiter);
    }

    [Fact]
    public void UserLimiter_WhenAccessed_ReturnsExpectedValue()
    {
        // Act & Assert
        Assert.Equal("userLimit", RateLimiters.UserLimiter);
    }

    [Fact]
    public void Concurrency_WhenAccessed_ReturnsExpectedValue()
    {
        // Act & Assert
        Assert.Equal("concurrency", RateLimiters.Concurrency);
    }
}
