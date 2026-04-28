using FitNova.Data;
using FitNova.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitNova.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;

        public IndexModel(UserManager<ApplicationUser> userManager, AppDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public int TotalUsers { get; set; }
        public int ProfilesCompleted { get; set; }
        public int TodayFoodLogs { get; set; }
        public int TodayWorkouts { get; set; }
        public List<UserStatRow> UserStats { get; set; } = new();

        public class UserStatRow
        {
            public string Email { get; set; } = "";
            public float WeightKg { get; set; }
            public float HeightCm { get; set; }
            public int Age { get; set; }
            public string Goal { get; set; } = "";
            public float TodayCalories { get; set; }
            public int WorkoutsThisWeek { get; set; }
            public bool IsAdmin { get; set; }
        }

        public async Task OnGetAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            TotalUsers = users.Count;

            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1);

            var profiles = await _db.UserProfiles.ToListAsync();
            var foodLogs = await _db.FoodLogs.Include(l => l.FoodItem).ToListAsync();
            var workouts = await _db.WorkoutLogs.ToListAsync();

            ProfilesCompleted = profiles.Count(p => p.WeightKg > 0);
            TodayFoodLogs = foodLogs.Count(l => l.Date.Date == today);
            TodayWorkouts = workouts.Count(w => w.Date.Date == today);

            foreach (var u in users)
            {
                var profile = profiles.FirstOrDefault(p => p.UserId == u.Id);
                var todayCal = foodLogs
                    .Where(l => l.UserId == u.Id && l.Date.Date == today)
                    .Sum(l => l.FoodItem?.Calories * l.Quantity ?? 0);
                var weekWorkouts = workouts.Count(w => w.UserId == u.Id && w.Date >= weekStart);
                var isAdmin = await _userManager.IsInRoleAsync(u, "Admin");

                UserStats.Add(new UserStatRow
                {
                    Email = u.Email ?? "",
                    WeightKg = profile?.WeightKg ?? 0,
                    HeightCm = profile?.HeightCm ?? 0,
                    Age = profile?.Age ?? 0,
                    Goal = profile?.Goal ?? "",
                    TodayCalories = todayCal,
                    WorkoutsThisWeek = weekWorkouts,
                    IsAdmin = isAdmin
                });
            }
        }
    }
}