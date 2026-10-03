using System.Security.Claims;
using App.Data;
using App.Infrastructure.Enums;
using App.Services.Context;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Auth;

public class DemoRoleMiddleware
{
    private readonly RequestDelegate _next;

    public DemoRoleMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx, AppDbContext db)
    {
        await ctx.Session.LoadAsync();

        var userId = ctx.Session.GetInt32(SessionAuthContext.UserIdKey);
        if (userId is null)
        {
            var domyslny = await db.Users.FirstOrDefaultAsync(u => u.Rola == RolaUzytkownika.Autor);
            if (domyslny is not null)
            {
                ctx.Session.SetInt32(SessionAuthContext.UserIdKey, domyslny.Id);
                ctx.Session.SetString(SessionAuthContext.RolaKey, domyslny.Rola.ToString());
                userId = domyslny.Id;
            }
        }

        if (userId is not null)
        {
            var rolaTxt = ctx.Session.GetString(SessionAuthContext.RolaKey) ?? RolaUzytkownika.Autor.ToString();
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
                new(ClaimTypes.Role, rolaTxt)
            };
            var identity = new ClaimsIdentity(claims, "demo");
            ctx.User = new ClaimsPrincipal(identity);
        }

        await _next(ctx);
    }
}
