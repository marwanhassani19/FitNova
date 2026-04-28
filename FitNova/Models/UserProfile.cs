namespace FitNova.Models;

public class UserProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";

    public float WeightKg { get; set; }
    public float HeightCm { get; set; }
    public int Age { get; set; }

    public string Goal { get; set; } = "maintain";
    public string ActivityLevel { get; set; }
    public string AiPlan { get; set; } = ""; // Il ? permette al valore di essere nullo
    public string WorkoutPlan { get; set; } = "";
}