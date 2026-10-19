using System.ComponentModel.DataAnnotations;
using HelthTrack.Controllers;
using HelthTrack.Filters;
using HelthTrack.Models;
using HelthTrack.Services;
using HelthTrack.ViewModels;
using Xunit;

namespace HealthTrack.Tests;

public class SecurityTests
{
    private readonly PasswordService _passwords = new();

    [Fact]
    public void CompatibilityMigrationDoesNotAddTheExistingColumnOrDropItOnRollback()
    {
        var migration = new HelthTrack.Migrations.AddPatientTypeToPatient();
        var operation = Assert.IsType<Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation>(Assert.Single(migration.UpOperations));
        Assert.Contains("IF COL_LENGTH", operation.Sql);
        Assert.Empty(migration.DownOperations);
    }

    [Fact]
    public void NewPasswordsAreSaltedAndVerifyWithoutPlaintext()
    {
        var user = new User { Username = "test" };
        user.Password = _passwords.Hash(user, "test-only-password");
        Assert.StartsWith("HT1:", user.Password);
        Assert.DoesNotContain("test-only-password", user.Password);
        Assert.NotEqual(user.Password, _passwords.Hash(user, "test-only-password"));
        Assert.True(_passwords.Verify(user, "test-only-password", out var upgrade));
        Assert.False(upgrade);
        Assert.False(_passwords.Verify(user, "wrong-password", out _));
    }

    [Fact]
    public void LegacyPasswordsRequireUpgradeOnlyAfterSuccessfulAuthentication()
    {
        var user = new User { Password = "legacy-test-only" };
        Assert.True(_passwords.Verify(user, "legacy-test-only", out var upgrade));
        Assert.True(upgrade);
        Assert.False(_passwords.Verify(user, "wrong", out _));
    }

    [Fact]
    public void MalformedHashDoesNotFallBackToPlaintext()
    {
        var user = new User { Password = "HT1:broken" };
        Assert.False(_passwords.Verify(user, "HT1:broken", out _));
    }

    [Theory]
    [InlineData(typeof(DoctorController))]
    [InlineData(typeof(PatientController))]
    public void EveryClinicalControllerHasARoleFilter(Type controller)
        => Assert.Single(controller.GetCustomAttributes(typeof(SessionRoleAttribute), true));

    [Fact]
    public void RegistrationRejectsInvalidEmailWeakPasswordAndFutureBirthDate()
    {
        var model = new RegisterPatientVM { FullName = "Test", Username = "test", Email = "invalid", Password = "short", ConfirmPassword = "different", DateOfBirth = DateTime.Today.AddDays(1), Gender = "Unknown", Disease = "Test" };
        var results = new List<ValidationResult>();
        Assert.False(Validator.TryValidateObject(model, new ValidationContext(model), results, true));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Email)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.Password)));
        model.Email = "demo@example.invalid"; model.Password = model.ConfirmPassword = "test-password"; model.Gender = "Male";
        results.Clear();
        Assert.False(Validator.TryValidateObject(model, new ValidationContext(model), results, true));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(model.DateOfBirth)));
    }
}
