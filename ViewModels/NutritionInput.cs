using System.ComponentModel.DataAnnotations;

namespace HelthTrack.ViewModels
{
    public class NutritionInputViewModel
    {
        [Required(ErrorMessage = "Height is required")]
        [Range(50, 250)]
        public double Height { get; set; }

        [Required(ErrorMessage = "Weight is required")]
        [Range(10, 400)]
        public double Weight { get; set; }

        [Required(ErrorMessage = "Activity level is required")]
        public string? ActivityLevel { get; set; } // Low / Moderate / High

        [Required(ErrorMessage = "Goal is required")]
        public string? Goal { get; set; } // Lose / Maintain / Gain
    }
}
