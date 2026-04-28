using FitNova.Data;
using FitNova.Models;
using FitNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FitNova.Pages.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _db;
        private readonly GeminiService _gemini;

        public IndexModel(UserManager<ApplicationUser> userManager, AppDbContext db, GeminiService gemini)
        {
            _userManager = userManager;
            _db = db;
            _gemini = gemini;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public UserProfile? Profile { get; set; }
        public string? AiPlan { get; set; }
        public string? SuccessMessage { get; set; }

        public class InputModel
        {
            public float WeightKg { get; set; }
            public float HeightCm { get; set; }
            public int Age { get; set; }
            public string Goal { get; set; } = "maintain";
            public string ActivityLevel { get; set; } = "moderate";
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            Profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (Profile != null)
            {
                Input.WeightKg = Profile.WeightKg;
                Input.HeightCm = Profile.HeightCm;
                Input.Age = Profile.Age;
                Input.Goal = Profile.Goal;
                Input.ActivityLevel = Profile.ActivityLevel;
                AiPlan = Profile.AiPlan;
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                profile = new UserProfile { UserId = user.Id };
                _db.UserProfiles.Add(profile);
            }

            profile.WeightKg = Input.WeightKg;
            profile.HeightCm = Input.HeightCm;
            profile.Age = Input.Age;
            profile.Goal = Input.Goal;
            profile.ActivityLevel = Input.ActivityLevel;

            await _db.SaveChangesAsync();

            SuccessMessage = "Profilo salvato con successo!";
            Profile = profile;
            AiPlan = profile.AiPlan;
            return Page();
        }

        public async Task<IActionResult> OnPostGeneratePlanAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var profile = await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
            if (profile == null)
            {
                SuccessMessage = "Salva prima il tuo profilo!";
                return Page();
            }

            var goalLabel = profile.Goal switch
            {
                "lose" => "perdita di peso",
                "gain" => "aumento massa muscolare",
                "maintain" => "mantenimento del peso",
                _ => "benessere generale"
            };

            var prompt = $@"Sei un nutrizionista professionista. Crea un piano nutrizionale settimanale dettagliato per una persona con questi dati:
- Peso: {profile.WeightKg} kg
- Altezza: {profile.HeightCm} cm  
- Età: {profile.Age} anni
- Obiettivo: {goalLabel}
- Livello attività: {profile.ActivityLevel}

Includi:
1. Fabbisogno calorico giornaliero
2. Suddivisione macronutrienti (proteine, carboidrati, grassi)
3. Piano pasti per 7 giorni (colazione, pranzo, cena, spuntini)
4. Consigli pratici

Rispondi in italiano, in modo chiaro e pratico.";

            var plan = await _gemini.Ask(prompt);
            profile.AiPlan = plan;
            await _db.SaveChangesAsync();

            Profile = profile;
            AiPlan = plan;
            SuccessMessage = "Piano generato con successo!";
            return Page();
        }
    }
}