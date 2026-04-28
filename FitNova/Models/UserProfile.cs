namespace FitNova.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";

    public float WeightKg { get; set; }
    public float HeightCm { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; } = "male";

    public string Goal { get; set; } = "maintain";
    public string ActivityLevel { get; set; } = "moderate";

    public string? NutritionPlan { get; set; }
    public string? WorkoutPlan { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}