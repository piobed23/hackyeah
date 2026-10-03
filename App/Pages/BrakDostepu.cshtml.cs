using App.Infrastructure.Auth;
using App.Services.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Pages;

[AllowAnonymous]
public class BrakDostepuModel : PageModel
{
    private readonly IAuthContext _auth;

    public BrakDostepuModel(IAuthContext auth) => _auth = auth;

    public string NazwaRoli { get; private set; } = "";
    public string Opis { get; private set; } = "";

    public void OnGet()
    {
        var rola = _auth.GetRola();
        NazwaRoli = Uprawnienia.Nazwa(rola);
        Opis = Uprawnienia.Opis(rola);
        Response.StatusCode = StatusCodes.Status403Forbidden;
    }
}
