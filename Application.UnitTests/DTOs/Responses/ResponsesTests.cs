using Application.DTOs.Responses.Answers;
using Application.DTOs.Responses.Authorization;
using Application.DTOs.Responses.Polls;
using Application.DTOs.Responses.Questions;
using Application.DTOs.Responses.Rolls;
using Application.DTOs.Responses.Users;
using Xunit;

namespace Application.UnitTests.DTOs.Responses;

public class ResponsesTests
{
    [Fact]
    public void AnswerResponse_WhenInstantiated_HoldsValues()
    {
        // Act
        var response = new AnswerResponse(1, "Option A");

        // Assert
        Assert.Equal(1, response.Id);
        Assert.Equal("Option A", response.Content);
    }

    [Fact]
    public void AuthResponse_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var expiresOn = DateTime.UtcNow.AddDays(7);

        // Act
        var response = new AuthResponse("user-1", "user@example.com", "John", "Doe", "token-xyz", 3600, "refresh-xyz", expiresOn);

        // Assert
        Assert.Equal("user-1", response.Id);
        Assert.Equal("user@example.com", response.Email);
        Assert.Equal("John", response.FirstName);
        Assert.Equal("Doe", response.LastName);
        Assert.Equal("token-xyz", response.Token);
        Assert.Equal(3600, response.ExpiresIn);
        Assert.Equal("refresh-xyz", response.RefreshToken);
        Assert.Equal(expiresOn, response.RefreshTokenExpiryDate);
    }

    [Fact]
    public void PollResponse_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var startsAt = new DateOnly(2026, 10, 1);
        var endsAt = new DateOnly(2026, 10, 31);

        // Act
        var response = new PollResponse(1, "Tech Survey", "Summary of survey", true, startsAt, endsAt);

        // Assert
        Assert.Equal(1, response.Id);
        Assert.Equal("Tech Survey", response.Title);
        Assert.Equal("Summary of survey", response.Summary);
        Assert.True(response.IsPublished);
        Assert.Equal(startsAt, response.StartsAt);
        Assert.Equal(endsAt, response.EndsAt);
    }

    [Fact]
    public void QuestionResponse_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var answers = new List<AnswerResponse> { new(1, "A1"), new(2, "A2") };

        // Act
        var response = new QuestionResponse(10, "What is your role?", answers);

        // Assert
        Assert.Equal(10, response.Id);
        Assert.Equal("What is your role?", response.Content);
        Assert.Same(answers, response.Answers);
    }

    [Fact]
    public void RoleDetailResponse_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var permissions = new List<string> { "polls:read", "polls:add" };

        // Act
        var response = new RoleDetailResponse("role-1", "Admin", false, permissions);

        // Assert
        Assert.Equal("role-1", response.Id);
        Assert.Equal("Admin", response.Name);
        Assert.False(response.IsDeleted);
        Assert.Same(permissions, response.Permissions);
    }

    [Fact]
    public void RoleResponse_WhenInstantiated_HoldsValues()
    {
        // Act
        var response = new RoleResponse("role-1", "Admin", false);

        // Assert
        Assert.Equal("role-1", response.Id);
        Assert.Equal("Admin", response.Name);
        Assert.False(response.IsDeleted);
    }

    [Fact]
    public void UserProfileResponse_WhenInstantiated_HoldsValues()
    {
        // Act
        var response = new UserProfileResponse("user@example.com", "user@example.com", "John", "Doe");

        // Assert
        Assert.Equal("user@example.com", response.Email);
        Assert.Equal("user@example.com", response.UserName);
        Assert.Equal("John", response.FirstName);
        Assert.Equal("Doe", response.LastName);
    }

    [Fact]
    public void UserResponse_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var roles = new List<string> { "Member" };

        // Act
        var response = new UserResponse("user-1", "John", "Doe", "john@example.com", false, roles);

        // Assert
        Assert.Equal("user-1", response.Id);
        Assert.Equal("John", response.FirstName);
        Assert.Equal("Doe", response.LastName);
        Assert.Equal("john@example.com", response.Email);
        Assert.False(response.IsDisabled);
        Assert.Same(roles, response.Roles);
    }
}
