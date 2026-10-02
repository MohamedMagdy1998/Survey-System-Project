using Application.DTOs.Requests.Polls;
using Application.DTOs.Requests.Questions;
using Application.DTOs.Requests.Rolls;
using Application.DTOs.Requests.Users;
using Application.DTOs.Requests.Votes;
using Xunit;

namespace Application.UnitTests.DTOs.Requests;

public class OtherRequestsTests
{
    [Fact]
    public void PollRequest_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var startsAt = new DateOnly(2026, 10, 1);
        var endsAt = new DateOnly(2026, 10, 15);

        // Act
        var request = new PollRequest("Poll 1", "Summary 1", startsAt, endsAt);

        // Assert
        Assert.Equal("Poll 1", request.Title);
        Assert.Equal("Summary 1", request.Summary);
        Assert.Equal(startsAt, request.StartsAt);
        Assert.Equal(endsAt, request.EndsAt);
    }

    [Fact]
    public void QuestionRequest_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var answers = new List<string> { "A1", "A2" };

        // Act
        var request = new QuestionRequest("Question 1", answers);

        // Assert
        Assert.Equal("Question 1", request.Content);
        Assert.Same(answers, request.Answers);
    }

    [Fact]
    public void RoleRequest_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var permissions = new List<string> { "polls:read", "polls:add" };

        // Act
        var request = new RoleRequest("Editor", permissions);

        // Assert
        Assert.Equal("Editor", request.Name);
        Assert.Same(permissions, request.Permissions);
    }

    [Fact]
    public void ChangePasswordRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new ChangePasswordRequest("OldPassword123!", "NewPassword123!");

        // Assert
        Assert.Equal("OldPassword123!", request.CurrentPassword);
        Assert.Equal("NewPassword123!", request.NewPassword);
    }

    [Fact]
    public void CreateUserRequest_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var roles = new List<string> { "Member", "Admin" };

        // Act
        var request = new CreateUserRequest("Jane", "Doe", "jane@example.com", "Password123!", roles);

        // Assert
        Assert.Equal("Jane", request.FirstName);
        Assert.Equal("Doe", request.LastName);
        Assert.Equal("jane@example.com", request.Email);
        Assert.Equal("Password123!", request.Password);
        Assert.Same(roles, request.Roles);
    }

    [Fact]
    public void UpdateProfileRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new UpdateProfileRequest("Jane", "Smith");

        // Assert
        Assert.Equal("Jane", request.FirstName);
        Assert.Equal("Smith", request.LastName);
    }

    [Fact]
    public void UpdateUserRequest_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var roles = new List<string> { "Admin" };

        // Act
        var request = new UpdateUserRequest("Jane", "Smith", "jane.smith@example.com", roles);

        // Assert
        Assert.Equal("Jane", request.FirstName);
        Assert.Equal("Smith", request.LastName);
        Assert.Equal("jane.smith@example.com", request.Email);
        Assert.Same(roles, request.Roles);
    }

    [Fact]
    public void VoteAnswerRequest_WhenInstantiated_HoldsValues()
    {
        // Act
        var request = new VoteAnswerRequest(10, 20);

        // Assert
        Assert.Equal(10, request.QuestionId);
        Assert.Equal(20, request.AnswerId);
    }

    [Fact]
    public void VoteRequest_WhenInstantiated_HoldsValues()
    {
        // Arrange
        var answers = new List<VoteAnswerRequest> { new(1, 2) };

        // Act
        var request = new VoteRequest(answers);

        // Assert
        Assert.Same(answers, request.Answers);
    }
}
