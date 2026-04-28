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

namespace FitNova.Pages.Profile;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly AppDbContext _db;
    private readonly GeminiService _gemini;

    public IndexModel(UserManager<ApplicationUser> um, AppDbContext db, GeminiService gemini)
    { _um = um; _db = db; _gemini = gemini; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public float WeightKg { get; set; }
        public float HeightCm { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = "male";
        public string Goal { get; set; } = "maintain";
        public string ActivityLevel { get; set; } = "moderate";
    }

    public UserProfile? Profile { get; set; }
    public string? NutritionPlan { get; set; }
    public string FormattedPlan { get; set; } = "";
    public string? Toast { get; set; }
    public bool ToastError { get; set; }

    public async Task OnGetAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return;
        Profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (Profile != null)
        {
            Input.WeightKg = Profile.WeightKg;
            Input.HeightCm = Profile.HeightCm;
            Input.Age = Profile.Age;
            Input.Gender = Profile.Gender;
            Input.Goal = Profile.Goal;
            Input.ActivityLevel = Profile.ActivityLevel;
            NutritionPlan = Profile.NutritionPlan;
            FormattedPlan = FormatPlan(Profile.NutritionPlan ?? "");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return RedirectToPage();

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null) { profile = new UserProfile { UserId = user.Id }; _db.UserProfiles.Add(profile); }

        profile.WeightKg = Input.WeightKg;
        profile.HeightCm = Input.HeightCm;
        profile.Age = Input.Age;
        profile.Gender = Input.Gender;
        profile.Goal = Input.Goal;
        profile.ActivityLevel = Input.ActivityLevel;
        profile.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        Profile = profile;
        NutritionPlan = profile.NutritionPlan;
        FormattedPlan = FormatPlan(NutritionPlan ?? "");
        Toast = "Profilo salvato con successo!";
        return Page();
    }

    public async Task<IActionResult> OnPostGeneratePlanAsync()
    {
        var user = await _um.GetUserAsync(User);
        if (user == null) return RedirectToPage();

        var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
        if (profile == null)
        {
            Toast = "Salva prima il tuo profilo!";
            ToastError = true;
            await OnGetAsync();
            return Page();
        }

        var goalLabel = profile.Goal switch
        {
            "lose" => "perdita di peso",
            "gain" => "aumento della massa muscolare",
            "maintain" => "mantenimento del peso",
            _ => "benessere generale"
        };
        var actLabel = profile.ActivityLevel switch
        {
            "sedentary" => "sedentario",
            "light" => "leggermente attivo",
            "moderate" => "moderatamente attivo",
            "active" => "molto attivo",
            _ => "moderato"
        };
        var genLabel = profile.Gender == "female" ? "donna" : "uomo";

        var prompt = $@"Sei un nutrizionista professionista certificato. Crea un piano nutrizionale settimanale completo in italiano per:

## Dati paziente
- Sesso: {genLabel}
- Età: {profile.Age} anni
- Peso: {profile.WeightKg} kg
- Altezza: {profile.HeightCm} cm
- Obiettivo: {goalLabel}
- Livello attività: {actLabel}

## Struttura richiesta (segui ESATTAMENTE questo formato)

**FABBISOGNO CALORICO GIORNALIERO**
Calcola TDEE e calorie target.

**MACRONUTRIENTI TARGET**
- Proteine: X g (Y%)
- Carboidrati: X g (Y%)
- Grassi: X g (Y%)

**PIANO SETTIMANALE**

### LUNEDÌ
**Colazione:** ...
**Pranzo:** ...
**Cena:** ...
**Spuntino:** ...
*Totale: ~X kcal*

### MARTEDÌ
[stesso formato per tutti i 7 giorni]

**CONSIGLI PRATICI**
3-4 consigli specifici per l'obiettivo.

Sii specifico con le grammature (es: 80g pasta, 150g petto di pollo).";

        var raw = await _gemini.Ask(prompt);

        profile.NutritionPlan = raw;
        profile.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();

        Profile = profile;
        NutritionPlan = raw;
        FormattedPlan = FormatPlan(raw);
        Toast = "Piano generato con successo!";
        return Page();
    }

    private static string FormatPlan(string raw)
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
        Regex.Replace(Regex.Replace(Esc(s), @"\*\*(.+?)\*\*", "<strong>$1</strong>"), @"\*(.+?)\*", "<em>$1</em>");

    private static string Esc(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}