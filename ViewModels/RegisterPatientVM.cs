using System.ComponentModel.DataAnnotations;

namespace HelthTrack.ViewModels;

public sealed class RegisterPatientVM : IValidatableObject
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, RegularExpression(@"[A-Za-z0-9_.-]{3,50}", ErrorMessage = "Use 3–50 letters, numbers, dots, hyphens, or underscores.")]
    public string Username { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 8), DataType(DataType.Password)]
    public string Password { get; set; } = "";
    [Required, Compare(nameof(Password)), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";
    [Required, DataType(DataType.Date)] public DateTime DateOfBirth { get; set; }
    [Required, RegularExpression("Male|Female")] public string Gender { get; set; } = "";
    [Required, StringLength(200)] public string Disease { get; set; } = "";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth > DateTime.Today || DateOfBirth < DateTime.Today.AddYears(-120))
            yield return new ValidationResult("Enter a valid date of birth within the last 120 years.", [nameof(DateOfBirth)]);
    }
}
