using FitNova.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FitNova.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _sm;
    public LogoutModel(SignInManager<ApplicationUser> sm) => _sm = sm;

    public async Task<IActionResult> OnPostAsync()
    {
        await _sm.SignOutAsync();
        return RedirectToPage("/Account/Login");
    }
}