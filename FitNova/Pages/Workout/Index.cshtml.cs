using FitNova.Data;
using FitNova.Models;
using FitNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace FitNova.Pages.Workout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;
        private readonly GeminiService _gemini;

        // UNICO COSTRUTTORE: Gestisce Database, Utenti e AI
        public IndexModel(UserManager<ApplicationUser> userManager, AppDbContext db, GeminiService gemini)
        {
            _userManager = userManager;
            _db = db;
            _gemini = gemini;
        }

        // Proprietà per la pagina Razor
        public string? WorkoutPlan { get; set; }
        public int WorkoutsThisWeek { get; set; }
        public int WeeklyGoal { get; set; } = 4;
        public int TotalWorkouts { get; set; }
        public int Streak { get; set; }
        public HashSet<int> WorkoutDays { get; set; } = new();
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            // Carica il profilo dell'utente per la scheda AI
            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            WorkoutPlan = profile?.WorkoutPlan;

            // Calcolo inizio settimana (Lunedì)
            var weekStart = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek == 0 ? 6 : (int)DateTime.Today.DayOfWeek - 1);
            var logs = await _db.WorkoutLogs.Where(w => w.UserId == user.Id).ToListAsync();

            TotalWorkouts = logs.Count;
            WorkoutsThisWeek = logs.Count(w => w.Date.Date >= weekStart.Date);

            // Popola i pallini della settimana
            foreach (var log in logs.Where(w => w.Date.Date >= weekStart.Date))
            {
                var dow = (int)log.Date.DayOfWeek;
                WorkoutDays.Add(dow == 0 ? 6 : dow - 1);
            }

            // Calcolo della Streak (giorni consecutivi)
            var streak = 0;
            for (int i = 0; i < 30; i++)
            {
                var day = DateTime.Today.AddDays(-i);
                if (logs.Any(w => w.Date.Date == day.Date)) streak++;
                else break;
            }
            Streak = streak;
        }

        public async Task<IActionResult> OnPostGeneratePlanAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);

            var goal = profile?.Goal ?? "maintain";
            var goalLabel = goal switch
            {
                "lose" => "perdita di peso e tonificazione",
                "gain" => "aumento della massa muscolare",
                "maintain" => "mantenimento e forma fisica",
                _ => "benessere generale"
            };

            // Prepariamo il prompt per l'AI
            var prompt = $@"Sei un personal trainer professionista. Crea una scheda di allenamento settimanale per obiettivo: {goalLabel}.
Peso: {profile?.WeightKg ?? 70} kg, Altezza: {profile?.HeightCm ?? 170} cm.

Crea una scheda 4 giorni a settimana con:
- Giorno e gruppo muscolare
- Esercizi con serie, ripetizioni e recupero
- Consigli su warm-up e cool-down

Rispondi in italiano, formato chiaro e pratico.";

            // CHIAMATA AL SERVIZIO GEMINI
            var plan = await _gemini.Ask(prompt);

            if (profile != null)
            {
                profile.WorkoutPlan = plan;
                await _db.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLogWorkoutAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            // Aggiunge un nuovo log di allenamento
            _db.WorkoutLogs.Add(new WorkoutLog
            {
                UserId = user.Id,
                Date = DateTime.Now,
                Notes = "Completato"
            });

            await _db.SaveChangesAsync();

            SuccessMessage = "Allenamento registrato! ??";

            // Ricarichiamo i dati aggiornati
            await OnGetAsync();
            return Page();
        }
    }
}