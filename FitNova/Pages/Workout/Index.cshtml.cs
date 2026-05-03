using FitNova.Data;
using FitNova.Models;
using FitNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace FitNova.Pages.Workout;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly AppDbContext _db;
    private readonly GeminiService _gemini;

    public IndexModel(UserManager<ApplicationUser> um,
                      AppDbContext db, GeminiService gemini)
    { _um = um; _db = db; _gemini = gemini; }

    public string? WorkoutPlan { get; set; }
    public string FormattedPlan { get; set; } = "";
    public int WorkoutsThisWeek { get; set; }
    public int WeeklyGoal { get; set; } = 4;
    public int TotalWorkouts { get; set; }
    public int Streak { get; set; }
    public HashSet<int> WorkoutDays { get; set; } = new();
    public string? Toast { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return;

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        WorkoutPlan = profile?.WorkoutPlan;
        FormattedPlan = FormatWorkout(WorkoutPlan ?? "");

        var weekStart = DateTime.Today
            .AddDays(-(((int)DateTime.Today.DayOfWeek + 6) % 7));

        var logs = await _db.WorkoutLogs
            .Where(w => w.UserId == user.Id)
            .ToListAsync();

        TotalWorkouts = logs.Count;
        WorkoutsThisWeek = logs.Count(w => w.Date >= weekStart);

        foreach (var log in logs.Where(w => w.Date >= weekStart))
            WorkoutDays.Add(((int)log.Date.DayOfWeek + 6) % 7);

        for (int i = 0; i < 30; i++)
        {
            if (logs.Any(w => w.Date.Date == DateTime.Today.AddDays(-i)))
                Streak++;
            else break;
        }
    }

    public async Task<IActionResult> OnPostGeneratePlanAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return RedirectToPage();

        var profile = await _db.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == user.Id);

        var goalLabel = (profile?.Goal) switch
        {
            "lose" => "dimagrimento e tonificazione",
            "gain" => "ipertrofia (aumento muscolare)",
            _ => "forma fisica generale"
        };

        var prompt = $@"Sei un personal trainer. Crea una scheda settimanale in italiano per: {goalLabel}.
Peso {profile?.WeightKg ?? 70}kg, Altezza {profile?.HeightCm ?? 170}cm.

### GIORNO 1 — NOME
**Esercizi:**
- Esercizio: X serie × X rip
### GIORNO 2 — NOME
[tutti i giorni]
**CONSIGLI**";

        var raw = await _gemini.Ask(prompt);

        if (profile != null)
        {
            profile.WorkoutPlan = raw;
            profile.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLogWorkoutAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return RedirectToPage();

        _db.WorkoutLogs.Add(new WorkoutLog
        {
            UserId = user.Id,
            Date = DateTime.Now
        });
        await _db.SaveChangesAsync();
        Toast = "Allenamento registrato! 💪";
        await OnGetAsync();
        return Page();
    }

    private static string FormatWorkout(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "";
        var sb = new StringBuilder();
        bool inDay = false;

        foreach (var line in raw.Split('\n'))
        {
            var l = line.TrimEnd();
            if (l.StartsWith("### "))
            {
                if (inDay) sb.AppendLine("</div></div>");
                sb.AppendLine($"<div class='plan-day-block'><div class='plan-day-title'>{Esc(l[4..])}</div><div class='plan-day-body'>");
                inDay = true;
            }
            else if (l.StartsWith("## ") || l.StartsWith("# "))
            {
                if (inDay) { sb.AppendLine("</div></div>"); inDay = false; }
                sb.AppendLine($"<h2>{Esc(l.TrimStart('#').Trim())}</h2>");
            }
            else if (l.StartsWith("- ") || l.StartsWith("* "))
                sb.AppendLine($"<p>• {Inline(l[2..])}</p>");
            else if (!string.IsNullOrWhiteSpace(l))
                sb.AppendLine($"<p>{Inline(l)}</p>");
        }
        if (inDay) sb.AppendLine("</div></div>");
        return sb.ToString();
    }

    private static string Inline(string s) =>
        Regex.Replace(
            Regex.Replace(Esc(s), @"\*\*(.+?)\*\*", "<strong>$1</strong>"),
            @"\*(.+?)\*", "<em>$1</em>");

    private static string Esc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}