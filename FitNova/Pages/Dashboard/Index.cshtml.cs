using FitNova.Data;
using FitNova.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitNova.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public IndexModel(UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public UserProfile? Profile { get; set; }
        public float TodayCalories { get; set; }
        public float CalorieGoal { get; set; } = 2000;
        public float TodayProtein { get; set; }
        public float TodayCarbs { get; set; }
        public float TodayFat { get; set; }
        public int ProteinPct { get; set; }
        public int CarbsPct { get; set; }
        public int FatPct { get; set; }
        public int WorkoutsThisWeek { get; set; }
        public List<FoodLog> TodayLogs { get; set; } = new();
        public List<string> WeightLabels { get; set; } = new();
        public List<float> WeightData { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            Profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            // Calcola goal calorie
            if (Profile != null)
            {
                CalorieGoal = Profile.Goal switch
                {
                    "lose" => 1600,
                    "gain" => 2800,
                    "maintain" => 2200,
                    _ => 2000
                };
            }

            // Log di oggi
            var today = DateTime.Today;
            TodayLogs = await _db.FoodLogs
                .Where(l => l.UserId == user.Id && l.Date.Date == today)
                .Include(l => l.FoodItem)
                .ToListAsync();

            TodayCalories = TodayLogs.Sum(l => l.FoodItem?.Calories * l.Quantity ?? 0);
            TodayProtein = TodayLogs.Sum(l => l.FoodItem?.Protein * l.Quantity ?? 0);
            TodayCarbs = TodayLogs.Sum(l => l.FoodItem?.Carbs * l.Quantity ?? 0);
            TodayFat = TodayLogs.Sum(l => l.FoodItem?.Fat * l.Quantity ?? 0);

            var totalMacros = TodayProtein * 4 + TodayCarbs * 4 + TodayFat * 9;
            if (totalMacros > 0)
            {
                ProteinPct = (int)(TodayProtein * 4 / totalMacros * 100);
                CarbsPct = (int)(TodayCarbs * 4 / totalMacros * 100);
                FatPct = 100 - ProteinPct - CarbsPct;
            }

            // Dati peso ultimi 7 giorni (placeholder — in produzione usa WeightLog)
            for (int i = 6; i >= 0; i--)
            {
                var d = DateTime.Today.AddDays(-i);
                WeightLabels.Add(d.ToString("ddd", new System.Globalization.CultureInfo("it-IT")));
                WeightData.Add(Profile?.WeightKg ?? 70);
            }

            // Workout questa settimana
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            WorkoutsThisWeek = await _db.WorkoutLogs
                .Where(w => w.UserId == user.Id && w.Date >= weekStart)
                .CountAsync();
        }
    }
}