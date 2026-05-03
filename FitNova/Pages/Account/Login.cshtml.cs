using FitNova.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace FitNova.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _sm;
    public LoginModel(SignInManager<ApplicationUser> sm) => _sm = sm;

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required] public string Email { get; set; } = "";
        [Required] public string Password { get; set; } = "";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var r = await _sm.PasswordSignInAsync(Input.Email, Input.Password, false, false);
        if (r.Succeeded) return RedirectToPage("/Dashboard/Index");
        ModelState.AddModelError("", "Email o password non corretti.");
        return Page();
    }
}