using System.ComponentModel.DataAnnotations;

namespace HelthTrack.ViewModels;

public sealed class LoginVM
{
    [Required, StringLength(50)] public string Username { get; set; } = "";
    [Required, StringLength(128), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, RegularExpression("Patient|Doctor")] public string SelectedRole { get; set; } = "Patient";
}
