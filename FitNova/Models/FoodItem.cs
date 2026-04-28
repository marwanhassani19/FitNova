namespace FitNova.Models;

public class FoodItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Barcode { get; set; } = "";
    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Carbs { get; set; }
    public float Fat { get; set; }
}