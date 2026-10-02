using Application.Services_Implementations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Application.UnitTests.Services_Implementations;

public class CacheServiceTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock = new();
    private readonly Mock<ILogger<CacheService>> _loggerMock = new();
    private readonly CacheService _sut;

    public CacheServiceTests()
    {
        _sut = new CacheService(_distributedCacheMock.Object, _loggerMock.Object);
    }

    private record SampleCacheItem(int Id, string Name);

    [Fact]
    public async Task GetAsync_WhenKeyExists_ReturnsDeserializedObject()
    {
        // Arrange
        var key = "test-key";
        var expectedItem = new SampleCacheItem(1, "Sample Name");
        var serialized = JsonSerializer.Serialize(expectedItem);
        var bytes = Encoding.UTF8.GetBytes(serialized);

        _distributedCacheMock
            .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _sut.GetAsync<SampleCacheItem>(key);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedItem.Id, result.Id);
        Assert.Equal(expectedItem.Name, result.Name);
    }

    [Fact]
    public async Task GetAsync_WhenKeyDoesNotExist_ReturnsDefaultNull()
    {
        // Arrange
        var key = "missing-key";

        _distributedCacheMock
            .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetAsync<SampleCacheItem>(key);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_WhenInvoked_SerializesAndStoresValueInCache()
    {
        // Arrange
        var key = "store-key";
        var itemToStore = new SampleCacheItem(42, "Value");

        // Act
        await _sut.SetAsync(key, itemToStore);

        // Assert
        _distributedCacheMock.Verify(
            c => c.SetAsync(
                key,
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b).Contains("Value")),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WhenInvoked_RemovesKeyFromCache()
    {
        // Arrange
        var key = "remove-key";

        // Act
        await _sut.RemoveAsync(key);

        // Assert
        _distributedCacheMock.Verify(
            c => c.RemoveAsync(key, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
