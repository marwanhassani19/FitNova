namespace FitNova.Models;

public class WorkoutLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public DateTime Date { get; set; } = DateTime.Now;
    public string Notes { get; set; } = "";
    public int Duration { get; set; } = 60;
}