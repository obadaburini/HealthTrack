namespace HelthTrack.Models
{
    public class NutritionResult
    {
        // BMI
        public double BMI { get; set; }
        public string? Category { get; set; }

        // Patient info
        public int Age { get; set; }
        public string? Gender { get; set; }

        // Calories
        public double BMR { get; set; }
        public double DailyCalories { get; set; }

        // Goal
        public string? Goal { get; set; }

        // Macros (grams/day)
        public double ProteinGrams { get; set; }
        public double CarbsGrams { get; set; }
        public double FatGrams { get; set; }
    }
}
