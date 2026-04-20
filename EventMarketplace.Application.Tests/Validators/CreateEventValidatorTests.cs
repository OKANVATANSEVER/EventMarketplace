using EventMarketplace.Application.Commands.Events;
using EventMarketplace.Application.Validators.Events;
using FluentAssertions;
using Xunit;

namespace EventMarketplace.Application.Tests.Validators;

public class CreateEventValidatorTests
{
    private readonly CreateEventValidator _validator = new();

    [Fact]
    public void Validate_Should_ReturnValid_WhenCommandIsCorrect()
    {
        var command = new CreateEventCommand(
            "Test Event",
            "Valid description",
            100,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            "Istanbul",
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            false);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_ReturnInvalid_WhenStartDateIsAfterEndDate()
    {
        var command = new CreateEventCommand(
            "Test Event",
            "Valid description",
            100,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(1),
            "Istanbul",
            Guid.NewGuid(),
            Guid.NewGuid().ToString(),
            false);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "StartDate");
    }
}
