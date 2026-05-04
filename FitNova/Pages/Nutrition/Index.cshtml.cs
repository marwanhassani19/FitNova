using FitNova.Data;
using FitNova.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitNova.Pages.Nutrition;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly AppDbContext _db;

    public IndexModel(UserManager<ApplicationUser> um, AppDbContext db)
    {
        _um = um;
        _db = db;
    }

    public List<FoodLog> Logs { get; set; } = new();
    public float TodayCalories { get; set; }
    public float TodayProtein { get; set; }
    public float TodayCarbs { get; set; }
    public float TodayFat { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return;
        var today = DateTime.Today;

        Logs = await _db.FoodLogs
            .Where(l => l.UserId == user.Id && l.Date.Date == today)
            .Include(l => l.FoodItem)
            .OrderBy(l => l.MealType).ThenBy(l => l.Date)
            .ToListAsync();

        TodayCalories = Logs.Sum(l => (l.FoodItem?.Calories ?? 0) * l.Quantity);
        TodayProtein = Logs.Sum(l => (l.FoodItem?.Protein ?? 0) * l.Quantity);
        TodayCarbs = Logs.Sum(l => (l.FoodItem?.Carbs ?? 0) * l.Quantity);
        TodayFat = Logs.Sum(l => (l.FoodItem?.Fat ?? 0) * l.Quantity);
    }

    public async Task<IActionResult> OnPostAddFoodAsync(
        string foodName, float calories, float protein, float carbs, float fat,
        float quantity = 1, string mealType = "pranzo")
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return RedirectToPage();

        var item = await _db.FoodItems.FirstOrDefaultAsync(f => f.Name == foodName);
        if (item == null)
        {
            item = new FoodItem { Name = foodName, Calories = calories, Protein = protein, Carbs = carbs, Fat = fat };
            _db.FoodItems.Add(item);
            await _db.SaveChangesAsync();
        }

        _db.FoodLogs.Add(new FoodLog
        {
            UserId = user.Id,
            FoodItemId = item.Id,
            Quantity = quantity,
            MealType = mealType,
            Date = DateTime.Now
        });
        await _db.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteLogAsync(int logId)
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return RedirectToPage();

        var log = await _db.FoodLogs.FirstOrDefaultAsync(l => l.Id == logId && l.UserId == user.Id);
        if (log != null)
        {
            _db.FoodLogs.Remove(log);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    // --- METODI PER LE API (Puliti e senza doppioni) ---

    public async Task<IActionResult> OnGetSearchGenericAsync(string q, [FromServices] FitNova.Services.FoodService foodService)
    {
        if (string.IsNullOrWhiteSpace(q)) return new JsonResult(new { });
        var json = await foodService.SearchGenericFood(q);
        return Content(json, "application/json");
    }

    public async Task<IActionResult> OnGetBarcodeAsync(string bc, [FromServices] FitNova.Services.FoodService foodService)
    {
        if (string.IsNullOrWhiteSpace(bc)) return new JsonResult(new { });
        var json = await foodService.SearchByBarcode(bc);
        return Content(json, "application/json");
    }
}