using Domain.Common.Abstractions;
using Domain.Common.Abstractions.Errors;
using Xunit;

namespace Domain.UnitTests.Common.Abstractions;

public class ResultTests
{
    private class TestResult : Result
    {
        public TestResult(bool isSuccess, Error error) : base(isSuccess, error) { }
    }

    private class TestResult<T> : Result<T>
    {
        public TestResult(T? value, bool isSuccess, Error error) : base(value, isSuccess, error) { }
    }

    [Fact]
    public void Success_WhenCalled_ReturnsSuccessResultWithNoError()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_WhenCalledWithCustomError_ReturnsFailureResultWithSpecifiedError()
    {
        // Arrange
        var customError = Error.BadRequest("Test.Error", "Something went wrong");

        // Act
        var result = Result.Failure(customError);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(customError, result.Error);
    }

    [Fact]
    public void Success_GenericWhenCalledWithValue_ReturnsGenericSuccessResultWithValue()
    {
        // Arrange
        const string expectedValue = "Success Value";

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Failure_GenericWhenCalledWithError_ReturnsGenericFailureResultWithError()
    {
        // Arrange
        var error = Error.NotFound("Entity.NotFound", "Entity not found");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Value_WhenResultIsSuccess_ReturnsAssignedValue()
    {
        // Arrange
        const int expectedValue = 42;
        var result = Result<int>.Success(expectedValue);

        // Act & Assert
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Value_WhenResultIsFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var error = Error.Conflict("Test.Conflict", "Conflict occurred");
        var result = Result<int>.Failure(error);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _ = result.Value);
        Assert.Equal("The value of a failure result cannot be accessed.", exception.Message);
    }

    [Fact]
    public void Constructor_WhenSuccessWithNonNoneError_ThrowsInvalidOperationException()
    {
        // Arrange
        var error = Error.BadRequest("Err", "Err Description");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new TestResult(true, error));
        Assert.Equal("A result cannot be successful with an error or a failure without an error.", exception.Message);
    }

    [Fact]
    public void Constructor_WhenFailureWithErrorNone_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new TestResult(false, Error.None));
        Assert.Equal("A result cannot be successful with an error or a failure without an error.", exception.Message);
    }

    [Fact]
    public void Constructor_GenericWhenSuccessWithNonNoneError_ThrowsInvalidOperationException()
    {
        // Arrange
        var error = Error.BadRequest("Err", "Err Description");

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new TestResult<string>("value", true, error));
        Assert.Equal("A result cannot be successful with an error or a failure without an error.", exception.Message);
    }

    [Fact]
    public void Constructor_GenericWhenFailureWithErrorNone_ThrowsInvalidOperationException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new TestResult<string>(null, false, Error.None));
        Assert.Equal("A result cannot be successful with an error or a failure without an error.", exception.Message);
    }

    [Fact]
    public void ImplicitOperator_WhenConvertingFromError_ReturnsFailureResult()
    {
        // Arrange
        var error = Error.Unauthorized("Auth.Unauthorized", "Unauthorized access");

        // Act
        Result result = error;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void ImplicitOperator_WhenConvertingFromValueToGenericResult_ReturnsSuccessResult()
    {
        // Arrange
        const string expectedValue = "ImplicitValue";

        // Act
        Result<string> result = expectedValue;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void ImplicitOperator_WhenConvertingFromErrorToGenericResult_ReturnsFailureResult()
    {
        // Arrange
        var error = Error.Forbidden("Auth.Forbidden", "Forbidden access");

        // Act
        Result<string> result = error;

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }
}
