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
    { _um = um; _db = db; }

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

    public async Task<IActionResult> OnGetSearchFoodAsync(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return new JsonResult(new { products = Array.Empty<object>() });
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "FitNova/1.0");
            // Cerca sia in italiano che internazionale
            var url = $"https://it.openfoodfacts.org/cgi/search.pl?search_terms={Uri.EscapeDataString(q)}&search_simple=1&action=process&json=1&page_size=8";
            var json = await http.GetStringAsync(url);
            return Content(json, "application/json");
        }
        catch
        {
            return new JsonResult(new { products = Array.Empty<object>() });
        }
    }

    public async Task<IActionResult> OnGetBarcodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return new JsonResult(new { status = 0 });
        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "FitNova/1.0");
            var url = $"https://world.openfoodfacts.org/api/v0/product/{code}.json";
            var json = await http.GetStringAsync(url);
            return Content(json, "application/json");
        }
        catch
        {
            return new JsonResult(new { status = 0 });
        }
    }

    public async Task<IActionResult> OnPostAddFoodAsync(
        string foodName, float calories, float protein,
        float carbs, float fat, float quantity = 1,
        string mealType = "pranzo")
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return RedirectToPage();

        var item = await _db.FoodItems.FirstOrDefaultAsync(f => f.Name == foodName);
        if (item == null)
        {
            item = new FoodItem
            {
                Name = foodName,
                Calories = calories,
                Protein = protein,
                Carbs = carbs,
                Fat = fat
            };
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

        var log = await _db.FoodLogs
            .FirstOrDefaultAsync(l => l.Id == logId && l.UserId == user.Id);
        if (log != null)
        {
            _db.FoodLogs.Remove(log);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}