using FitNova.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FitNova.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly SignInManager<ApplicationUser> _sm;

    public RegisterModel(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm)
    { _um = um; _sm = sm; }

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string Email { get; set; } = "";
        [Required, MinLength(6)] public string Password { get; set; } = "";
        [Required, Compare("Password")] public string ConfirmPassword { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var user = new ApplicationUser { UserName = Input.Email, Email = Input.Email };
        var r = await _um.CreateAsync(user, Input.Password);
        if (r.Succeeded)
        {
            await _um.AddToRoleAsync(user, "User");
            await _sm.SignInAsync(user, false);
            return RedirectToPage("/Dashboard/Index");
        }
        foreach (var e in r.Errors) ModelState.AddModelError("", e.Description);
        return Page();
    }
}