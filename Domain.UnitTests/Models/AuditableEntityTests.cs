using Domain.Models;
using Xunit;

namespace Domain.UnitTests.Models;

public class AuditableEntityTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesDefaults()
    {
        // Act
        var entity = new AuditableEntity();

        // Assert
        Assert.Equal(string.Empty, entity.CreatedById);
        Assert.Equal(default, entity.CreatedOn);
        Assert.Null(entity.UpdatedById);
        Assert.Equal(default, entity.UpdatedOn);
        Assert.Null(entity.UpdatedBy);
    }

    [Fact]
    public void Properties_WhenAssigned_StoreAssignedValues()
    {
        // Arrange
        var entity = new AuditableEntity();
        var createdUser = new ApplicationUser { Id = "creator-1" };
        var updatedUser = new ApplicationUser { Id = "updater-1" };
        var now = DateTime.UtcNow;
        var later = now.AddHours(1);

        // Act
        entity.CreatedById = "creator-1";
        entity.CreatedOn = now;
        entity.CreatedBy = createdUser;
        entity.UpdatedById = "updater-1";
        entity.UpdatedOn = later;
        entity.UpdatedBy = updatedUser;

        // Assert
        Assert.Equal("creator-1", entity.CreatedById);
        Assert.Equal(now, entity.CreatedOn);
        Assert.Same(createdUser, entity.CreatedBy);
        Assert.Equal("updater-1", entity.UpdatedById);
        Assert.Equal(later, entity.UpdatedOn);
        Assert.Same(updatedUser, entity.UpdatedBy);
    }
}
