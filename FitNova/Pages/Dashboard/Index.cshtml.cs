using FitNova.Data;
using FitNova.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitNova.Pages.Dashboard;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly AppDbContext _db;

    public IndexModel(UserManager<ApplicationUser> um, AppDbContext db) { _um = um; _db = db; }

    public float TodayCalories { get; set; }
    public float CalorieGoal { get; set; } = 2000;
    public float TodayProtein { get; set; }
    public float TodayCarbs { get; set; }
    public float TodayFat { get; set; }
    public float TotalMacroKcal { get; set; }
    public float WeightKg { get; set; }
    public int WorkoutsThisWeek { get; set; }
    public string GoalLabel { get; set; } = "—";

    public List<FoodLog> TodayLogs { get; set; } = new();
    public List<string> WeightLabels { get; set; } = new();
    public List<float> WeightValues { get; set; } = new();

    public async Task OnGetAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return;

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        WeightKg = profile?.WeightKg ?? 0;

        if (profile != null)
        {
            CalorieGoal = profile.Goal switch
            {
                "lose" => 1600,
                "gain" => 2800,
                "maintain" => 2200,
                _ => 2000
            };
            GoalLabel = profile.Goal switch
            {
                "lose" => "Dimagrire",
                "gain" => "Massa",
                "maintain" => "Mantenimento",
                _ => "—"
            };
        }

        var today = DateTime.Today;
        TodayLogs = await _db.FoodLogs
            .Where(l => l.UserId == user.Id && l.Date.Date == today)
            .Include(l => l.FoodItem)
            .OrderByDescending(l => l.Date)
            .ToListAsync();

        TodayCalories = TodayLogs.Sum(l => (l.FoodItem?.Calories ?? 0) * l.Quantity);
        TodayProtein = TodayLogs.Sum(l => (l.FoodItem?.Protein ?? 0) * l.Quantity);
        TodayCarbs = TodayLogs.Sum(l => (l.FoodItem?.Carbs ?? 0) * l.Quantity);
        TodayFat = TodayLogs.Sum(l => (l.FoodItem?.Fat ?? 0) * l.Quantity);
        TotalMacroKcal = TodayProtein * 4 + TodayCarbs * 4 + TodayFat * 9;

        var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        WorkoutsThisWeek = await _db.WorkoutLogs
            .Where(w => w.UserId == user.Id && w.Date >= weekStart)
            .CountAsync();

        for (int i = 6; i >= 0; i--)
        {
            var d = today.AddDays(-i);
            WeightLabels.Add(d.ToString("ddd", new System.Globalization.CultureInfo("it-IT")));
            WeightValues.Add(WeightKg > 0 ? WeightKg : 70f);
        }
    }
}