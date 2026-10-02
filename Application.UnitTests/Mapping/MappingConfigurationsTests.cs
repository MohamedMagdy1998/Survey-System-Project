using Application.DTOs.Requests.Questions;
using Application.Mapping;
using Domain.Models;
using Mapster;
using Xunit;

namespace Application.UnitTests.Mapping;

public class MappingConfigurationsTests
{
    [Fact]
    public void Register_WhenMappingQuestionRequestToQuestion_IgnoresAnswersCollection()
    {
        // Arrange
        var config = new TypeAdapterConfig();
        var mappingConfig = new MappingConfigurations();
        mappingConfig.Register(config);

        var request = new QuestionRequest(
            Content: "How satisfied are you?",
            Answers: new List<string> { "Very Satisfied", "Neutral", "Unsatisfied" }
        );

        // Act
        var question = request.Adapt<Question>(config);

        // Assert
        Assert.NotNull(question);
        Assert.Equal("How satisfied are you?", question.Content);
        Assert.Empty(question.Answers);
    }
}
