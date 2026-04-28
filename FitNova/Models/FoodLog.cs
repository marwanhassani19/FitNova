namespace FitNova.Models;

public class FoodLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }
    public float Quantity { get; set; } = 1f;
    public string MealType { get; set; } = "pranzo";
    public DateTime Date { get; set; } = DateTime.Now;
}