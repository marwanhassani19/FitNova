namespace FitNova.Models;

public class FoodLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public int FoodItemId { get; set; }
    public FoodItem? FoodItem { get; set; }
    public float Quantity { get; set; } = 1; // multiplier of 100g
    public DateTime Date { get; set; } = DateTime.Now;

    // Convenience props
    public string FoodName => FoodItem?.Name ?? "";
    public float Calories => FoodItem?.Calories ?? 0;
    public float Protein => FoodItem?.Protein ?? 0;
    public float Carbs => FoodItem?.Carbs ?? 0;
    public float Fat => FoodItem?.Fat ?? 0;
}