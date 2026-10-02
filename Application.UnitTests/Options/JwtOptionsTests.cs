using Application.Options;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Application.UnitTests.Options;

public class JwtOptionsTests
{
    [Fact]
    public void Constructor_DefaultValues_InitializesExpectedDefaults()
    {
        // Act
        var options = new JwtOptions();

        // Assert
        Assert.Equal("JwtSettings", JwtOptions.SectionName);
        Assert.Equal(string.Empty, options.Key);
        Assert.Equal(string.Empty, options.Issuer);
        Assert.Equal(string.Empty, options.Audience);
        Assert.Equal(0, options.ExpiryInMinutes);
    }

    [Fact]
    public void Properties_WhenAssigned_StoresAssignedValues()
    {
        // Act
        var options = new JwtOptions
        {
            Key = "super_secret_jwt_key_that_is_long_enough",
            Issuer = "SurveyBasketIssuer",
            Audience = "SurveyBasketAudience",
            ExpiryInMinutes = 60
        };

        // Assert
        Assert.Equal("super_secret_jwt_key_that_is_long_enough", options.Key);
        Assert.Equal("SurveyBasketIssuer", options.Issuer);
        Assert.Equal("SurveyBasketAudience", options.Audience);
        Assert.Equal(60, options.ExpiryInMinutes);
    }

    [Fact]
    public void Validation_WhenPropertiesAreEmpty_FailsDataAnnotationValidation()
    {
        // Arrange
        var options = new JwtOptions { Key = "", Issuer = "", Audience = "" };
        var context = new ValidationContext(options);
        var validationResults = new List<ValidationResult>();

        // Act
        var isValid = Validator.TryValidateObject(options, context, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
    }
}
