using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity!.IsAuthenticated)
            return RedirectToPage("/Dashboard/Index");

        return RedirectToPage("/Account/Login");
    }
}