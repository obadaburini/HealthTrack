using System;
using System.ComponentModel.DataAnnotations;

namespace HelthTrack.Models
{
    public class PatientUser
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        public double? Weight { get; set; }

        public double? Height { get; set; }

        public string Disease { get; set; } = "";

        [Required(ErrorMessage = "Gender is required")]
        public string Gender { get; set; } = "";

        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = "";
    }
}
